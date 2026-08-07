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

    // ─── site.nav 渲染树 ──────────────────────────────────

    [Fact]
    public void BuildNavTree_FlatPages_NoDirNodes()
    {
        var pages = new List<PageModel>
        {
            new() { Title = "首页", Url = "/" },
            new() { Title = "关于", Url = "/about" },
        };

        var nav = SiteGenerator.BuildNavTree(pages);

        Assert.Equal(2, nav.Count);
        Assert.Equal("/", nav[0].Url);
        Assert.Equal("关于", nav[1].Title);
        Assert.Empty(nav[0].Children);
    }

    [Fact]
    public void BuildNavTree_NestedPages_CreatesDirNodes()
    {
        var pages = new List<PageModel>
        {
            new() { Title = "首页", Url = "/" },
            new() { Title = "第一篇", Url = "/blog/post-1" },
            new() { Title = "第二篇", Url = "/blog/post-2" },
            new() { Title = "关于", Url = "/about" },
        };

        var nav = SiteGenerator.BuildNavTree(pages);

        // 按 URL 字典序：/、/about、/blog/* —— 顶层为 首页、关于、blog 目录
        Assert.Equal(3, nav.Count);
        Assert.Equal("/", nav[0].Url);
        Assert.Equal("关于", nav[1].Title);
        var blog = nav[2];
        Assert.Equal("blog", blog.Title);
        Assert.Null(blog.Url); // 目录节点
        Assert.Equal(2, blog.Children.Count);
        Assert.Equal("/blog/post-1", blog.Children[0].Url);
        Assert.Equal("/blog/post-2", blog.Children[1].Url);
    }

    [Fact]
    public void BuildNavTree_LanguagePages_StripsLangPrefix()
    {
        var pages = _generator.LoadPages(_sourceDir, "en"); // /en/ 与 /en/about

        var nav = SiteGenerator.BuildNavTree(pages, "en");

        // 剥离 /en 前缀后：根层级直接是 首页 + about，没有 en 目录节点
        Assert.Equal(2, nav.Count);
        Assert.Contains(nav, n => n.Url == "/en/");
        Assert.Contains(nav, n => n.Url == "/en/about");
        Assert.DoesNotContain(nav, n => n.Url is null); // 无目录节点
    }

    [Fact]
    public void BuildNavTree_NonLanguagePages_KeepsDirs()
    {
        var pages = _generator.LoadPages(_sourceDir, null); // 只有 blog/post

        var nav = SiteGenerator.BuildNavTree(pages);

        Assert.Single(nav);
        Assert.Equal("blog", nav[0].Title);
        Assert.Null(nav[0].Url);
        Assert.Single(nav[0].Children);
        Assert.Equal("/blog/post", nav[0].Children[0].Url);
    }

    // ─── baseUrl（GitHub Pages 子路径部署）──────────────

    [Fact]
    public void Build_WithBaseUrl_PrefixesLinks()
    {
        // 找到源码默认主题（测试输出目录不含 Themes）
        var themeDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "PicoSite", "Themes", "default"));
        if (!Directory.Exists(themeDir))
            return; // 源目录不可用时跳过

        var config = new SiteConfig { BaseUrl = "/PicoSite/", DefaultLanguage = "zh" };
        var outDir = Path.Combine(_root, "_site");
        var gen = new SiteGenerator(new MarkdownParser(), new TemplateEngine(themeDir), config);
        gen.Build(_sourceDir, outDir);

        // 默认语言 zh 输出到站点根（符合静态站点惯例）
        var html = File.ReadAllText(Path.Combine(outDir, "index.html"));

        // baseUrl 尾部斜杠规范化，链接无双斜杠
        Assert.Contains("/PicoSite/about", html);
        Assert.DoesNotContain("/PicoSite//", html);
    }

    // ─── 语言级 site.json 配置 ──────────────────────────

    [Fact]
    public void LoadLanguageSite_ReadsLanguageSiteJson()
    {
        WriteMd(Path.Combine(_sourceDir, "en", "site.json"),
            "{\n  \"title\": \"English Site\",\n  \"description\": \"English desc\"\n}");

        var (title, desc) = _generator.LoadLanguageSite(_sourceDir, "en");

        Assert.Equal("English Site", title);
        Assert.Equal("English desc", desc);
    }

    [Fact]
    public void LoadLanguageSite_NoSiteJson_ReturnsNull()
    {
        // zh 目录没有 site.json
        var (title, desc) = _generator.LoadLanguageSite(_sourceDir, "zh");

        Assert.Null(title);
        Assert.Null(desc);
    }

    // ─── build 输出结构 + 语言标题 + 同页切换 ──────────

    [Fact]
    public void Build_DefaultLanguageToRoot_OtherToSubdir()
    {
        var themeDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "PicoSite", "Themes", "default"));
        if (!Directory.Exists(themeDir)) return;

        WriteMd(Path.Combine(_sourceDir, "zh", "site.json"),
            "{\n  \"title\": \"中文站\"\n}");
        WriteMd(Path.Combine(_sourceDir, "en", "site.json"),
            "{\n  \"title\": \"English Site\"\n}");

        var config = new SiteConfig { DefaultLanguage = "zh", BaseUrl = "/PicoSite/" };
        var outDir = Path.Combine(_root, "_site");
        var gen = new SiteGenerator(new MarkdownParser(), new TemplateEngine(themeDir), config);
        gen.Build(_sourceDir, outDir);

        // 默认语言 zh 输出到根，非默认 en 输出到子目录
        Assert.True(File.Exists(Path.Combine(outDir, "index.html")), "默认语言应输出到根");
        Assert.True(File.Exists(Path.Combine(outDir, "en", "index.html")), "其他语言应输出到子目录");

        // 语言级标题生效
        var zhHtml = File.ReadAllText(Path.Combine(outDir, "index.html"));
        Assert.Contains("中文站", zhHtml);

        // 语言切换器同页互切：zh 的 about 页切 en → /en/about
        var zhAbout = File.ReadAllText(Path.Combine(outDir, "about.html"));
        Assert.Contains("/PicoSite/en/about", zhAbout);
    }
}
