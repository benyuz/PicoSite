using PicoSite.Models;

namespace PicoSite.Services;

public class SiteGenerator
{
    private readonly MarkdownParser _parser;
    private readonly TemplateEngine _templates;
    private readonly SiteConfig _config;

    // ISO 639-1 常用语言代码（用于自动识别语言目录）
    private static readonly HashSet<string> IsoLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "af", "am", "ar", "az", "be", "bg", "bn", "bs", "ca", "cs", "cy", "da", "de",
        "el", "en", "es", "et", "fa", "fi", "fr", "ga", "he", "hi", "hr", "hu", "hy",
        "id", "is", "it", "ja", "ka", "kk", "km", "ko", "ky", "lt", "lv", "mk", "mn",
        "ms", "my", "ne", "nl", "no", "pa", "pl", "pt", "ro", "ru", "si", "sk", "sl",
        "sq", "sr", "sv", "sw", "ta", "te", "th", "tl", "tr", "uk", "ur", "uz", "vi", "zh"
    };

    /// <summary>
    /// 默认语言代码（配置优先，否则取第一个检测到的语言目录）。
    /// 注意：需要在确定源目录后使用 ResolveDefaultLanguage 获取最终值。
    /// </summary>
    public string DefaultLanguage { get; }

    public SiteGenerator(MarkdownParser parser, TemplateEngine templates, SiteConfig config)
    {
        _parser = parser;
        _templates = templates;
        _config = config;
        DefaultLanguage = config.DefaultLanguage ?? "";
    }

    // ─── 语言检测 ──────────────────────────────────────────

    /// <summary>检测源目录下的一级语言目录（目录名为 ISO 语言代码）。</summary>
    public static List<string> DetectLanguages(string sourceDir)
    {
        if (!Directory.Exists(sourceDir)) return new List<string>();

        return Directory.GetDirectories(sourceDir)
            .Select(Path.GetFileName)
            .Where(n => n is not null && IsoLanguages.Contains(n))
            .Cast<string>()
            .OrderBy(n => n)
            .ToList();
    }

    /// <summary>是否为语言目录名。</summary>
    public static bool IsLanguageCode(string name) =>
        name is not null && IsoLanguages.Contains(name);

    /// <summary>
    /// 语言代码 → 本地显示名（切换按钮用原语言名显示）。
    /// 未收录的代码返回自身。
    /// </summary>
    public static string LanguageDisplayName(string code) =>
        LanguageNames.TryGetValue(code, out var name) ? name : code;

    private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh"] = "中文",
        ["en"] = "English",
        ["ja"] = "日本語",
        ["ko"] = "한국어",
        ["fr"] = "Français",
        ["de"] = "Deutsch",
        ["es"] = "Español",
        ["ru"] = "Русский",
        ["pt"] = "Português",
        ["it"] = "Italiano",
        ["ar"] = "العربية",
        ["hi"] = "हिन्दी",
        ["nl"] = "Nederlands",
        ["pl"] = "Polski",
        ["tr"] = "Türkçe",
        ["vi"] = "Tiếng Việt",
        ["th"] = "ไทย",
        ["uk"] = "Українська",
    };

    /// <summary>解析最终默认语言：配置优先，否则取第一个检测到的语言目录。</summary>
    public string ResolveDefaultLanguage(string sourceDir)
    {
        if (!string.IsNullOrEmpty(_config.DefaultLanguage))
            return _config.DefaultLanguage;
        return DetectLanguages(sourceDir).FirstOrDefault() ?? "";
    }

    /// <summary>
    /// 读取语言目录下的 site.json（可选，覆盖该语言的站点级配置）。
    /// 当前只支持 title/description 按语言区分。
    /// </summary>
    public (string? Title, string? Description) LoadLanguageSite(string sourceDir, string? language)
    {
        if (language is null) return (null, null);
        var path = Path.Combine(sourceDir, language, "site.json");
        if (!File.Exists(path)) return (null, null);

        try
        {
            var json = File.ReadAllText(path);
            // 用源码生成 context（项目禁用 JSON 反射，AOT 兼容）
            var cfg = System.Text.Json.JsonSerializer.Deserialize(
                json, PicoSiteJsonContext.Default.SiteConfig);
            return (cfg?.Title, cfg?.Description);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[警告] 无法解析语言配置 {path}: {ex.Message}");
            return (null, null);
        }
    }

    /// <summary>
    /// 加载页面。
    /// language 为 null 时返回非语言页面（排除语言目录）；
    /// 指定语言时返回该语言目录下的全部页面。
    /// </summary>
    public List<PageModel> LoadPages(string sourceDir, string? language = null)
    {
        var pages = new List<PageModel>();
        if (!Directory.Exists(sourceDir)) return pages;

        var languages = DetectLanguages(sourceDir);
        var searchRoot = language is not null ? Path.Combine(sourceDir, language) : sourceDir;

        foreach (var file in Directory.GetFiles(searchRoot, "*.md", SearchOption.AllDirectories))
        {
            // 非语言模式：跳过所有语言目录下的文件
            if (language is null && IsInsideAnyLanguageDir(file, sourceDir, languages))
                continue;

            var page = ParseFile(file, sourceDir, language);
            if (page is not null) pages.Add(page);
        }

        return pages.OrderBy(p => p.Url).ToList();
    }

    /// <summary>加载全部页面（语言 + 非语言），用于统计和热重载。</summary>
    public List<PageModel> LoadAllPages(string sourceDir)
    {
        var pages = LoadPages(sourceDir, null);
        foreach (var lang in DetectLanguages(sourceDir))
            pages.AddRange(LoadPages(sourceDir, lang));
        return pages.OrderBy(p => p.Url).ToList();
    }

    /// <summary>
    /// 按页面 URL 的目录层级构建导航渲染树。
    /// 目录节点 Url 为 null，页面节点 Url 为完整路径。
    /// language 非空时先剥离 URL 的语言前缀（/en/about → /about），
    /// 使导航层级以语言目录为站点根。
    /// </summary>
    public static List<NavNode> BuildNavTree(List<PageModel> pages, string? language = null)
    {
        var root = new List<NavNode>();
        var dirs = new Dictionary<string, NavNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var page in pages.OrderBy(p => p.Url))
        {
            var url = page.Url;

            // 剥离语言前缀：语言模式下 URL 形如 /en/about
            if (language is not null && url.StartsWith("/" + language + "/", StringComparison.OrdinalIgnoreCase))
                url = url[(language.Length + 1)..]; // /en/about → /about

            var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 首页（/）直接挂根
            if (segments.Length == 0)
            {
                root.Add(new NavNode { Title = page.Title, Url = page.Url, Date = page.Date });
                continue;
            }

            // 逐级定位/创建目录节点
            var parent = root;
            var path = "";
            for (var i = 0; i < segments.Length - 1; i++)
            {
                path += "/" + segments[i];
                if (!dirs.TryGetValue(path, out var dirNode))
                {
                    dirNode = new NavNode { Title = segments[i], Url = null };
                    dirs[path] = dirNode;
                    parent.Add(dirNode);
                }
                parent = dirNode.Children;
            }

            parent.Add(new NavNode { Title = page.Title, Url = page.Url, Date = page.Date });
        }

        return root;
    }

    public PageModel? LoadPage(string sourceDir, string requestPath, string? language = null)
    {
        // 将请求路径转为可能的 .md 文件路径
        var relative = requestPath.TrimStart('/');
        if (string.IsNullOrEmpty(relative)) relative = "index";

        // 搜索根：语言目录优先，回退源目录（兼容非语言页面）
        var searchRoots = new List<string>();
        if (language is not null)
            searchRoots.Add(Path.Combine(sourceDir, language));
        searchRoots.Add(sourceDir);

        var candidates = new[]
        {
            relative + ".md",
            Path.Combine(relative, "index.md"),
        };

        foreach (var root in searchRoots)
        {
            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(Path.Combine(root, candidate));
                var rootFull = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(rootFull, StringComparison.Ordinal))
                    continue;

                if (File.Exists(fullPath))
                    return ParseFile(fullPath, sourceDir, language);
            }
        }

        return null;
    }

    private PageModel? ParseFile(string filePath, string sourceDir, string? language = null)
    {
        try
        {
            var markdown = File.ReadAllText(filePath);
            var (frontMatter, html) = _parser.Parse(markdown);

            // 文件是否真的位于语言目录内（LoadPage 回退到根目录时可能不在）
            var langDir = language is not null ? Path.Combine(sourceDir, language) : null;
            var inLangDir = langDir is not null && IsInside(filePath, langDir);

            // 语言页面以语言目录为基准计算相对路径，否则以源目录为基准
            var baseDir = inLangDir ? langDir! : sourceDir;
            var relative = Path.GetRelativePath(baseDir, filePath)
                .Replace('\\', '/')
                .Replace(".md", "");

            if (relative.EndsWith("/index")) relative = relative[..^6];
            else if (relative == "index") relative = "";

            var url = "/" + relative;
            // 非默认语言页面加语言前缀（index 页带尾部斜杠：/en/）
            if (inLangDir)
            {
                var defaultLang = ResolveDefaultLanguage(sourceDir);
                if (!string.IsNullOrEmpty(defaultLang)
                    && !string.Equals(language, defaultLang, StringComparison.OrdinalIgnoreCase))
                    url = relative.Length > 0 ? $"/{language}/{relative}" : $"/{language}/";
            }

            var page = new PageModel
            {
                Title = frontMatter?.GetValueOrDefault("title")?.ToString()
                         ?? Path.GetFileNameWithoutExtension(filePath),
                Url = url,
                Content = html,
                SourcePath = filePath,
                FrontMatter = frontMatter,
            };

            if (frontMatter?.TryGetValue("date", out var dateObj) == true
                && DateTime.TryParse(dateObj.ToString(), out var date))
                page.Date = date;

            page.Excerpt = ExtractExcerpt(html);
            return page;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[警告] 无法解析文件 {filePath}: {ex.Message}");
            return null;
        }
    }

    private static string? ExtractExcerpt(string html)
    {
        const string tag = "<p>";
        var start = html.IndexOf(tag, StringComparison.Ordinal);
        if (start < 0) return null;

        var end = html.IndexOf("</p>", start, StringComparison.Ordinal);
        if (end < 0) return null;

        start += tag.Length;
        var excerpt = html[start..end];
        // 截断时避开 HTML 标签边界
        if (excerpt.Length <= 150) return excerpt;

        // 先剥离 HTML 标签再截断
        var textOnly = System.Text.RegularExpressions.Regex.Replace(excerpt, "<[^>]+>", "");
        return textOnly.Length > 150 ? textOnly[..150] + "..." : textOnly;
    }

    private static bool IsInsideAnyLanguageDir(string file, string sourceDir, List<string> languages)
    {
        foreach (var lang in languages)
        {
            if (IsInside(file, Path.Combine(sourceDir, lang)))
                return true;
        }
        return false;
    }

    private static bool IsInside(string file, string dir)
    {
        var fileFull = Path.GetFullPath(file);
        var dirFull = Path.GetFullPath(dir) + Path.DirectorySeparatorChar;
        return fileFull.StartsWith(dirFull, StringComparison.Ordinal);
    }

    // ─── Build 模式 ──────────────────────────────────────────

    public void Build(string sourceDir, string outputDir)
    {
        var languages = DetectLanguages(sourceDir);
        // 未配置默认语言时，取第一个检测到的语言目录
        var defaultLang = !string.IsNullOrEmpty(_config.DefaultLanguage)
            ? _config.DefaultLanguage
            : languages.FirstOrDefault() ?? "";

        Directory.CreateDirectory(outputDir);

        // 1) 非语言页面 → 输出目录根
        var rootPages = LoadPages(sourceDir, null);
        if (rootPages.Count > 0)
        {
            var rootSite = new SiteModel
            {
                Title = _config.Title ?? "PicoSite",
                Description = _config.Description,
                Language = null,
                Languages = languages,
                DefaultLanguage = defaultLang,
                BaseUrl = _config.BaseUrl ?? "",
                Github = _config.Github,
                Email = _config.Email,
                Pages = rootPages,
            };

            foreach (var page in rootPages)
                RenderPageToFile(page, rootSite, outputDir, null);
        }

        // 2) 各语言 → 默认语言输出到站点根（符合静态站点惯例），其他语言输出到子目录
        foreach (var lang in languages)
        {
            var langOutput = lang == defaultLang
                ? outputDir
                : Path.Combine(outputDir, lang);
            Directory.CreateDirectory(langOutput);

            // 语言级站点配置（site.json 覆盖 title/description）
            var (langTitle, langDesc) = LoadLanguageSite(sourceDir, lang);

            var pages = LoadPages(sourceDir, lang);
            var site = new SiteModel
            {
                Title = langTitle ?? _config.Title ?? "PicoSite",
                Description = langDesc ?? _config.Description,
                Language = lang,
                Languages = languages,
                DefaultLanguage = defaultLang,
                BaseUrl = _config.BaseUrl ?? "",
                Github = _config.Github,
                Email = _config.Email,
                Pages = pages,
            };

            foreach (var page in pages)
                RenderPageToFile(page, site, langOutput, lang);
        }

        CopyThemeAssets(outputDir);

        // 404 页：渲染主题 404.html 到输出根（GitHub Pages 等项目站需要根级 404.html）
        if (File.Exists(Path.Combine(_templates.ThemeDir, "404.html")))
        {
            var pages404 = LoadPages(sourceDir, defaultLang);
            var (t404, d404) = LoadLanguageSite(sourceDir, defaultLang);
            var site404 = new SiteModel
            {
                Title = t404 ?? _config.Title ?? "PicoSite",
                Description = d404 ?? _config.Description,
                Language = string.IsNullOrEmpty(defaultLang) ? null : defaultLang,
                Languages = languages,
                DefaultLanguage = defaultLang,
                BaseUrl = _config.BaseUrl ?? "",
                Github = _config.Github,
                Email = _config.Email,
                Pages = pages404,
            };
            var errorHtml = _templates.Render("404", site404, new PageModel { Title = "404", Url = "/404.html" }, "");
            File.WriteAllText(Path.Combine(outputDir, "404.html"), errorHtml);
        }
    }

    private void RenderPageToFile(PageModel page, SiteModel site, string outputDir, string? language = null)
    {
        try
        {
            var html = _templates.RenderForPage(site, page, page.Content);

            // 语言站点输出时剥离 URL 的语言前缀（/en/about → /about），
            // 因为已经位于 outputDir/<lang>/ 下
            var url = page.Url;
            if (language is not null && url.StartsWith("/" + language + "/", StringComparison.OrdinalIgnoreCase))
                url = url[(language.Length + 1)..];

            var outPath = ResolveOutputPath(outputDir, url);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            File.WriteAllText(outPath, html);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 页面 \"{page.Url}\" 生成失败: {ex.Message}");
        }
    }

    private static string ResolveOutputPath(string outputDir, string url)
    {
        if (string.IsNullOrEmpty(url) || url == "/")
            return Path.Combine(outputDir, "index.html");

        var relative = url.TrimStart('/');
        if (!relative.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            relative += ".html";

        return Path.Combine(outputDir, relative);
    }

    private void CopyThemeAssets(string outputDir)
    {
        var themeAssetsDir = Path.Combine(AppContext.BaseDirectory, "Themes", _config.Theme ?? "default", "assets");
        if (!Directory.Exists(themeAssetsDir)) return;

        var dest = Path.Combine(outputDir, "themes", _config.Theme ?? "default", "assets");
        CopyDirectory(themeAssetsDir, dest);
    }

    private static void CopyDirectory(string src, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        foreach (var file in Directory.GetFiles(src))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);

        foreach (var dir in Directory.GetDirectories(src))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }

    // ─── 源目录自动检测 ──────────────────────────────────────

    public static string FindSourceDir(string workingDir)
    {
        foreach (var dir in new[] { "content", "docs", "." })
        {
            var full = Path.Combine(workingDir, dir);
            if (Directory.Exists(full) &&
                Directory.GetFiles(full, "*.md", SearchOption.AllDirectories).Length > 0)
                return Path.GetFullPath(full);
        }
        return workingDir;
    }
}
