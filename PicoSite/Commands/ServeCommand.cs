using System.CommandLine;
using System.Net;
using PicoServer;
using PicoSite.Models;
using PicoSite.Services;

namespace PicoSite.Commands;

public class ServeCommand : Command
{
    public ServeCommand(ConfigLoader configLoader, MarkdownParser markdownParser)
        : base("serve", "启动预览服务器（支持热重载）")
    {
        var portOption = new Option<int>("--port")
        {
            Description = "预览端口",
        };

        var themeOption = new Option<string>("--theme")
        {
            Description = "指定主题",
        };

        var themeDirOption = new Option<string>("--theme-dir")
        {
            Description = "主题目录路径（默认 exe 同目录下的 Themes/）",
        };

        AddOption(portOption);
        AddOption(themeOption);
        AddOption(themeDirOption);

        this.SetHandler(async (int port, string theme, string themeDir) =>
        {
            var config = configLoader.Load(Directory.GetCurrentDirectory());
            if (port > 0) config.Port = port;
            if (!string.IsNullOrEmpty(theme)) config.Theme = theme;

            await RunServer(config, markdownParser, themeDir);
        }, portOption, themeOption, themeDirOption);
    }

    private static async Task RunServer(SiteConfig config, MarkdownParser parser, string? themeDirOverride = null)
    {
        var workingDir = Directory.GetCurrentDirectory();
        var sourceDir = SiteGenerator.FindSourceDir(workingDir);
        var themeDir = !string.IsNullOrEmpty(themeDirOverride)
            ? Path.GetFullPath(themeDirOverride)
            : Path.Combine(AppContext.BaseDirectory, "Themes", config.Theme ?? "default");

        Console.WriteLine($"PicoSite v1.0");
        Console.WriteLine($"源目录: {sourceDir}");
        Console.WriteLine($"主题: {config.Theme}");
        Console.WriteLine();

        var app = new WebAPIServer();
        app.enableWebSocket = true;
        app.WsOnConnectionChanged = (_, _) => Task.CompletedTask;

        // 全局服务实例
        var templateEngine = new TemplateEngine(themeDir);
        var generator = new SiteGenerator(parser, templateEngine, config);

        // 多语言：检测语言目录，按语言构建各自的 SiteModel
        var languages = SiteGenerator.DetectLanguages(sourceDir);
        var defaultLang = config.DefaultLanguage ?? languages.FirstOrDefault() ?? "";

        var allPages = generator.LoadAllPages(sourceDir);
        if (allPages.Count == 0)
        {
            Console.WriteLine($"⚠️  未找到任何 Markdown 内容文件。");
            Console.WriteLine($"   当前源目录: {sourceDir}");
            Console.WriteLine($"   提示: 请切换到包含 content/ 的目录（如 sample/），");
            Console.WriteLine($"         或参考 README 创建内容。");
            Console.WriteLine();
        }

        // 各语言 site 缓存（key: 语言代码；空串 = 非语言页面）
        var sites = new Dictionary<string, SiteModel>(StringComparer.OrdinalIgnoreCase);
        sites[""] = BuildSite(config, generator, sourceDir, null, languages, defaultLang);
        foreach (var lang in languages)
            sites[lang] = BuildSite(config, generator, sourceDir, lang, languages, defaultLang);

        if (languages.Count > 0)
            Console.WriteLine($"多语言: {string.Join(", ", languages)}（默认: {defaultLang}）");

        // 热重载（文件变更时刷新站点数据 + 广播 WebSocket）
        var hotReload = new HotReloadService(app, sourceDir, () =>
        {
            sites[""] = BuildSite(config, generator, sourceDir, null, languages, defaultLang);
            foreach (var lang in languages)
                sites[lang] = BuildSite(config, generator, sourceDir, lang, languages, defaultLang);
            Console.WriteLine($"[热重载] 已刷新页面");
        });

        // 静态资源：主题 CSS/JS 等
        var themeAssetsDir = Path.Combine(themeDir, "assets");
        if (Directory.Exists(themeAssetsDir))
        {
            app.AddStaticFiles("/themes/" + config.Theme + "/assets", themeAssetsDir);
        }

        // 中间件：处理所有页面请求
        app.AddMiddleware(async (req, res) =>
        {
            try
            {
                var path = req.Url?.AbsolutePath ?? "/";

                // 放行静态资源和 WebSocket
                if (path.StartsWith("/themes/") || path == "/ws-reload")
                    return true;

                // 处理页面
                if (path.EndsWith("/index")) path = path[..^"index".Length];

                // 解析语言：非默认语言前缀路由到对应语言，其余走默认语言
                var (language, langPath) = ResolveLanguage(path, languages, defaultLang);
                var site = sites.TryGetValue(language ?? "", out var s) ? s : sites[""];

                await RenderPage(res, langPath, generator, templateEngine, site, sourceDir, themeDir);
                return false; // 已处理，终止后续
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[错误] {ex.Message}");
                res.StatusCode = 500;
                await res.WriteAsync("500 - Internal Server Error", "text/plain; charset=utf-8");
                return false;
            }
        });

        app.AddCors();

        // 启动
        Console.WriteLine($"预览: http://localhost:{config.Port}");
        Console.WriteLine("按 Ctrl+C 停止\n");

        try
        {
            app.StartServer(config.Port);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 端口 {config.Port} 被占用: {ex.Message}");
            hotReload.Dispose();
            return;
        }

        // 进程保活
        var exit = new ManualResetEventSlim(false);
        Console.CancelKeyPress += (_, _) =>
        {
            Console.WriteLine("正在停止...");
            app.StopServer();
            hotReload.Dispose();
            exit.Set();
        };
        exit.Wait();
    }

    private static SiteModel BuildSite(SiteConfig config, SiteGenerator generator, string sourceDir,
        string? language, List<string> languages, string defaultLang)
    {
        return new SiteModel
        {
            Title = config.Title ?? "PicoSite",
            Description = config.Description,
            Language = language,
            Languages = languages,
            DefaultLanguage = defaultLang,
            Pages = generator.LoadPages(sourceDir, language),
        };
    }

    /// <summary>
    /// 从请求路径解析语言：路径首段是非默认语言代码时路由到该语言并剥离前缀；
    /// 否则归默认语言（无语言目录时归非语言页面）。
    /// </summary>
    private static (string? Language, string Path) ResolveLanguage(string path, List<string> languages, string defaultLang)
    {
        var trimmed = path.TrimStart('/');
        var parts = trimmed.Split('/', 2);
        var first = parts[0];

        if (languages.Contains(first, StringComparer.OrdinalIgnoreCase)
            && !string.Equals(first, defaultLang, StringComparison.OrdinalIgnoreCase))
        {
            var rest = parts.Length > 1 ? "/" + parts[1] : "/";
            return (first, rest);
        }

        return (string.IsNullOrEmpty(defaultLang) ? null : defaultLang, path);
    }

    private static async Task RenderPage(
        HttpListenerResponse res,
        string path,
        SiteGenerator generator,
        TemplateEngine templateEngine,
        SiteModel site,
        string sourceDir,
        string themeDir)
    {
        var page = generator.LoadPage(sourceDir, path, site.Language);
        if (page is null)
        {
            res.StatusCode = 404;
            var errorHtml = await File.ReadAllTextAsync(Path.Combine(themeDir, "404.html"));
            await res.WriteAsync(errorHtml, "text/html; charset=utf-8");
            return;
        }

        var html = templateEngine.RenderForPage(site, page, page.Content);
        html = InjectReloadScript(html);
        await res.WriteAsync(html, "text/html; charset=utf-8");
    }

    private static string InjectReloadScript(string html)
    {
        const string script =
            "<script>" +
            "(function(){" +
            "var ws=new WebSocket('ws://'+location.host+'/ws-reload');" +
            "ws.onmessage=function(e){'reload'===e.data&&location.reload()};" +
            "ws.onerror=function(){console.warn('[热重载] WebSocket 连接失败')};" +
            "ws.onclose=function(){console.warn('[热重载] WebSocket 已关闭')};" +
            "})()" +
            "</script>";

        var idx = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? html.Insert(idx, script) : html + script;
    }
}
