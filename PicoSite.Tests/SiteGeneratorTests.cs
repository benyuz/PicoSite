using PicoSite.Models;
using PicoSite.Services;
using Xunit;

namespace PicoSite.Tests;

public class SiteGeneratorTests : IDisposable
{
    private readonly string _root;
    private readonly string _sourceDir;
    private readonly SiteConfig _config;
    private readonly SiteGenerator _generator;

    public SiteGeneratorTests()
    {
        // 构造临时内容目录：
        //   content/zh/index.md, about.md  （默认语言）
        //   content/en/index.md, about.md  （其他语言）
        //   content/blog/post.md           （非语言目录）
        _root = Path.Combine(Path.GetTempPath(), "picosite-test-" + Guid.NewGuid().ToString("N"));
        _sourceDir = Path.Combine(_root, "content");

        WriteMd(Path.Combine(_sourceDir, "zh", "index.md"), "---\ntitle: 首页\n---\n\n# 中文首页");
        WriteMd(Path.Combine(_sourceDir, "zh", "about.md"), "---\ntitle: 关于\n---\n\n# 关于我们");
        WriteMd(Path.Combine(_sourceDir, "en", "index.md"), "---\ntitle: Home\n---\n\n# English Home");
        WriteMd(Path.Combine(_sourceDir, "en", "about.md"), "---\ntitle: About\n---\n\n# About Us");
        WriteMd(Path.Combine(_sourceDir, "blog", "post.md"), "---\ntitle: 博文\n---\n\n# 博客文章");

        _config = new SiteConfig { DefaultLanguage = "zh" };
        var parser = new MarkdownParser();
        var templates = new TemplateEngine(Path.Combine(_root, "themes"));
        _generator = new SiteGenerator(parser, templates, _config);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch { /* 忽略清理失败 */ }
    }

    private static void WriteMd(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void DetectLanguages_DetectsIsoDirs_IgnoresOthers()
    {
        var languages = SiteGenerator.DetectLanguages(_sourceDir);

        Assert.Equal(new[] { "en", "zh" }, languages.OrderBy(l => l).ToArray());
    }

    [Fact]
    public void LoadPages_DefaultLanguage_UrlsWithoutPrefix()
    {
        var pages = _generator.LoadPages(_sourceDir, "zh");

        Assert.Equal(2, pages.Count);
        Assert.Contains(pages, p => p.Url == "/");
        Assert.Contains(pages, p => p.Url == "/about");
    }

    [Fact]
    public void LoadPages_OtherLanguage_UrlsWithPrefix()
    {
        var pages = _generator.LoadPages(_sourceDir, "en");

        Assert.Equal(2, pages.Count);
        Assert.Contains(pages, p => p.Url == "/en/");
        Assert.Contains(pages, p => p.Url == "/en/about");
    }

    [Fact]
    public void LoadPages_NonLanguage_ExcludesLangDirs()
    {
        var pages = _generator.LoadPages(_sourceDir, null);

        Assert.Single(pages);
        Assert.Equal("/blog/post", pages[0].Url);
    }

    [Fact]
    public void LoadPage_DefaultLanguage_ResolvesPage()
    {
        var page = _generator.LoadPage(_sourceDir, "/about", "zh");

        Assert.NotNull(page);
        Assert.Equal("关于", page!.Title);
        Assert.Equal("/about", page.Url);
    }

    [Fact]
    public void LoadPage_OtherLanguage_ResolvesPrefixedPage()
    {
        var page = _generator.LoadPage(_sourceDir, "/about", "en");

        Assert.NotNull(page);
        Assert.Equal("About", page!.Title);
        Assert.Equal("/en/about", page.Url);
    }

    [Fact]
    public void LoadPage_NonLanguage_FallsBackToRoot()
    {
        var page = _generator.LoadPage(_sourceDir, "/blog/post", "zh");

        Assert.NotNull(page);
        Assert.Equal("博文", page!.Title);
        Assert.Equal("/blog/post", page.Url);
    }
}
