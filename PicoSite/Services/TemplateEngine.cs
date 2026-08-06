using System.Text.RegularExpressions;
using Fluid;
using PicoSite.Models;

namespace PicoSite.Services;

public class TemplateEngine
{
    private readonly FluidParser _parser = new();
    private readonly string _themeDir;

    public TemplateEngine(string themeDir)
    {
        _themeDir = themeDir;
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
        context.SetValue("site", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = site.Title,
            ["description"] = site.Description ?? "",
            ["language"] = site.Language ?? "",
            ["default_language"] = site.DefaultLanguage ?? "",
            ["languages"] = site.Languages ?? new List<string>(),
            ["pages"] = site.Pages.Select(p => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Title"] = p.Title,
                ["Url"] = p.Url
            }).ToList(),
            ["nav"] = SiteGenerator.BuildNavTree(site.Pages, site.Language)
                .Select(NavToDict).ToList()
        });
        context.SetValue("page", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = page.Title,
            ["url"] = page.Url,
            ["date"] = page.Date?.ToString("yyyy-MM-dd") ?? "",
            ["excerpt"] = page.Excerpt ?? ""
        });
        context.SetValue("current_url", page.Url);
        context.SetValue("content", content);
        context.SetValue("theme", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["assets"] = "/themes/" + Path.GetFileName(_themeDir) + "/assets"
        });

        return template.Render(context);
    }

    private static Dictionary<string, object> NavToDict(NavNode node)
    {
        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Title"] = node.Title,
            ["Url"] = node.Url ?? "",
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
