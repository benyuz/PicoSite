using System.Text.RegularExpressions;
using Fluid;
using PicoSite.Models;

namespace PicoSite.Services;

public class TemplateEngine
{
    private readonly FluidParser _parser = new();
    private readonly string _themeDir;

    /// <summary>主题目录（供生成器定位 404.html 等主题文件）。</summary>
    public string ThemeDir => _themeDir;

    public TemplateEngine(string themeDir)
    {
        _themeDir = themeDir;
    }

    /// <summary>
    /// 按页面选择模板渲染：首页（语言内 URL 为 /）用 index.html，
    /// 其他页面用 page.html；主题无 page.html 时回退 index.html（兼容）。
    /// </summary>
    public string RenderForPage(SiteModel site, PageModel page, string content)
    {
        var templateName = IsHomePage(page, site) ? "index" : "page";
        if (templateName == "page" && !File.Exists(Path.Combine(_themeDir, "page.html")))
            templateName = "index";
        return Render(templateName, site, page, content);
    }

    private static bool IsHomePage(PageModel page, SiteModel site)
    {
        var url = page.Url;
        if (site.Language is not null
            && url.StartsWith("/" + site.Language + "/", StringComparison.OrdinalIgnoreCase))
            url = url[(site.Language.Length + 1)..];
        return url == "/";
    }

    /// <summary>
    /// 当前页面的语言内路径（URL 去掉语言前缀）：/quickstart、/en/quickstart → /quickstart。
    /// 语言切换器用它拼接目标语言的同页 URL。
    /// </summary>
    private static string LanguageInnerPath(PageModel page, SiteModel site)
    {
        var url = page.Url;
        if (site.Language is not null
            && url.StartsWith("/" + site.Language + "/", StringComparison.OrdinalIgnoreCase))
            url = url[(site.Language.Length + 1)..];
        return url;
    }

    public string Render(string templateName, SiteModel site, PageModel page, string content)
    {
        var path = Path.Combine(_themeDir, $"{templateName}.html");
        if (!File.Exists(path))
            throw new FileNotFoundException($"主题模板缺失: {path}");

        var source = File.ReadAllText(path);
        source = ResolveIncludes(source);

        if (!_parser.TryParse(source, out var template, out var error))
            throw new Exception($"模板解析失败: {error}");

        var options = new TemplateOptions();
        var context = new TemplateContext(options);
        var siteDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = site.Title,
            ["description"] = site.Description ?? "",
            ["language"] = site.Language ?? "",
            ["default_language"] = site.DefaultLanguage ?? "",
            ["base_url"] = (site.BaseUrl ?? "").TrimEnd('/'),
            ["current_path"] = LanguageInnerPath(page, site),
            ["github"] = site.Github ?? "",
            ["email"] = site.Email ?? "",
            ["languages"] = (site.Languages ?? new List<string>())
                .Select(l => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["code"] = l,
                    ["name"] = SiteGenerator.LanguageDisplayName(l)
                }).ToList(),
            ["pages"] = site.Pages.Select(p => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Title"] = p.Title,
                ["Url"] = p.Url,
                ["Excerpt"] = p.Excerpt ?? "",
                ["Date"] = p.Date?.ToString("yyyy-MM-dd") ?? ""
            }).ToList(),
            ["nav"] = SiteGenerator.BuildNavTree(site.Pages, site.Language)
                .Select(NavToDict).ToList()
        };
        // 自定义变量：picosite.json / site.json 中的任意字段 → {{ site.xxx }}
        foreach (var kv in site.Variables ?? new Dictionary<string, object>())
        {
            if (!siteDict.ContainsKey(kv.Key))
                siteDict[kv.Key] = kv.Value;
        }
        context.SetValue("site", siteDict);

        var pageDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = page.Title,
            ["url"] = page.Url,
            ["date"] = page.Date?.ToString("yyyy-MM-dd") ?? "",
            ["excerpt"] = page.Excerpt ?? ""
        };
        // 自定义变量：front matter 中的任意字段 → {{ page.xxx }}
        if (page.FrontMatter is not null)
        {
            foreach (var kv in page.FrontMatter)
            {
                if (!pageDict.ContainsKey(kv.Key))
                    pageDict[kv.Key] = kv.Value;
            }
        }
        context.SetValue("page", pageDict);
        context.SetValue("current_url", page.Url);
        context.SetValue("content", content);
        context.SetValue("theme", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["assets"] = "/themes/" + Path.GetFileName(_themeDir) + "/assets",
            ["i18n"] = GetThemeI18n(site.Language)
        });

        return template.Render(context);
    }

    /// <summary>
    /// 主题界面文案（导航/面包屑/页脚/404 等），按当前语言返回。
    /// 中文（zh*）用中文，其余语言回退英文。
    /// </summary>
    private static Dictionary<string, string> GetThemeI18n(string? language)
    {
        var isZh = language?.StartsWith("zh", StringComparison.OrdinalIgnoreCase) == true;
        return isZh
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["nav"] = "导航",
                ["docs"] = "文档",
                ["home"] = "首页",
                ["browse_docs"] = "浏览文档",
                ["getting_started"] = "快速上手",
                ["getting_started_desc"] = "在项目目录运行以下命令，即可在浏览器中预览站点：",
                ["feature_desc"] = "浏览 {title} 相关文档，了解详细内容和使用指南。",
                ["switch_language"] = "切换语言",
                ["search_placeholder"] = "搜索...",
                ["no_results"] = "未找到相关页面",
                ["last_updated"] = "最后更新",
                ["edit_this_page"] = "编辑此页",
                ["prev_page"] = "上一篇",
                ["next_page"] = "下一篇",
                ["table_of_contents"] = "本页目录",
                ["generated_by_prefix"] = "由",
                ["generated_by_suffix"] = "生成",
                ["not_found_title"] = "页面未找到",
                ["not_found_desc"] = "你访问的页面不存在或链接有误",
                ["back_home"] = "返回首页",
                ["back_prev"] = "返回上页",
                ["project"] = "项目",
                ["github_repo"] = "GitHub 仓库",
                ["releases"] = "发布与下载",
                ["contact_us"] = "联系我们",
            }
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["nav"] = "Navigation",
                ["docs"] = "Docs",
                ["home"] = "Home",
                ["browse_docs"] = "Browse Docs",
                ["getting_started"] = "Getting Started",
                ["getting_started_desc"] = "Run the command below in your project directory to preview your site:",
                ["feature_desc"] = "Browse the {title} documentation for details and usage guides.",
                ["switch_language"] = "Switch language",
                ["search_placeholder"] = "Search...",
                ["no_results"] = "No results found",
                ["last_updated"] = "Last updated",
                ["edit_this_page"] = "Edit this page",
                ["prev_page"] = "Previous",
                ["next_page"] = "Next",
                ["table_of_contents"] = "On this page",
                ["generated_by_prefix"] = "Generated by",
                ["generated_by_suffix"] = "",
                ["not_found_title"] = "Page Not Found",
                ["not_found_desc"] = "The page you are looking for does not exist.",
                ["back_home"] = "Back to Home",
                ["back_prev"] = "Go Back",
                ["project"] = "Project",
                ["github_repo"] = "GitHub Repository",
                ["releases"] = "Releases & Downloads",
                ["contact_us"] = "Contact Us",
            };
    }

    private static Dictionary<string, object> NavToDict(NavNode node)
    {
        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Title"] = node.Title,
            ["Url"] = node.Url ?? "",
            ["Date"] = node.Date?.ToString("yyyy-MM-dd") ?? "",
            ["Description"] = node.Description,
            ["Children"] = node.Children.Select(NavToDict).ToList()
        };
    }

    private string ResolveIncludes(string source)
    {
        // 手动解析 {% include "filename.html" %} 或 {% include 'filename.html' %}
        // 将引用的 .html 文件内容内联进来，避免 Fluid 自动追加 .liquid 后缀
        return Regex.Replace(source, @"{%\s*include\s+[""']([^""']+)[""']\s*%}", match =>
        {
            var includeFile = match.Groups[1].Value;
            var includePath = Path.Combine(_themeDir, includeFile);
            if (File.Exists(includePath))
                return File.ReadAllText(includePath);
            // 如果找不到，保留原样以便调试
            return match.Value;
        });
    }
}
