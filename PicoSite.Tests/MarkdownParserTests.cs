using PicoSite.Services;
using Xunit;

namespace PicoSite.Tests;

public class MarkdownParserTests
{
    private readonly MarkdownParser _parser = new();

    [Fact]
    public void Parse_PlainMarkdown_ReturnsHtmlWithoutFrontMatter()
    {
        var (frontMatter, html) = _parser.Parse("## Hello\n\nWorld");

        Assert.Null(frontMatter);
        Assert.Contains("<h2>Hello</h2>", html);
        Assert.Contains("<p>World</p>", html);
    }

    [Fact]
    public void Parse_MarkdownWithFrontMatter_ExtractsTitle()
    {
        var md = "---\ntitle: 测试文章\n---\n\n## 正文";

        var (frontMatter, html) = _parser.Parse(md);

        Assert.NotNull(frontMatter);
        Assert.Equal("测试文章", frontMatter!["title"]?.ToString());
        Assert.Contains("<h2>正文</h2>", html);
    }

    [Fact]
    public void Parse_FrontMatterWithCRLF_WorksCorrectly()
    {
        var md = "---\r\ntitle: CRLF测试\r\ndate: 2026-06-09\r\n---\r\n\r\n## CRLF 标题";

        var (frontMatter, html) = _parser.Parse(md);

        Assert.NotNull(frontMatter);
        Assert.Equal("CRLF测试", frontMatter!["title"]?.ToString());
        Assert.Contains("<h2>CRLF 标题</h2>", html);
    }

    [Fact]
    public void Parse_FrontMatterWithoutDate_DateNull()
    {
        var md = "---\ntitle: 无日期\n---\n\n内容";

        var (frontMatter, html) = _parser.Parse(md);

        Assert.NotNull(frontMatter);
        Assert.Equal("无日期", frontMatter!["title"]?.ToString());
        Assert.DoesNotContain("date", frontMatter!.Keys);
    }

    [Fact]
    public void Parse_PipeTable_RendersHtmlTable()
    {
        var md = "| 名称 | 说明 |\n| --- | --- |\n| PicoSite | 静态站点生成器 |";

        var (_, html) = _parser.Parse(md);

        Assert.Contains("<table>", html);
        Assert.Contains("<thead>", html);
        Assert.Contains("<tbody>", html);
        Assert.Contains("<td>PicoSite</td>", html);
        Assert.Contains("<td>静态站点生成器</td>", html);
    }

    [Fact]
    public void Parse_AutoLink_RendersAnchor()
    {
        var md = "官网： https://github.com/benyuz/PicoSite";

        var (_, html) = _parser.Parse(md);

        Assert.Contains("<a href=\"https://github.com/benyuz/PicoSite\">", html);
    }

    // ─── 公式 / 任务列表 / 删除线 / emoji / mermaid ──────────

    [Fact]
    public void Parse_InlineMath_RendersMathSpan()
    {
        var (_, html) = _parser.Parse("质能方程 $E=mc^2$ 是著名的公式。");

        Assert.Contains("class=\"math\"", html);
        Assert.Contains("E=mc^2", html);
    }

    [Fact]
    public void Parse_BlockMath_RendersMathBlock()
    {
        var (_, html) = _parser.Parse("$$\n\\int_0^1 x^2 dx\n$$");

        Assert.Contains("<div class=\"math\">", html);
        Assert.Contains("\\int_0^1", html);
    }

    [Fact]
    public void Parse_TaskList_RendersCheckboxes()
    {
        var md = "- [x] 已完成任务\n- [ ] 未完成任务";

        var (_, html) = _parser.Parse(md);

        Assert.Contains("task-list-item", html);
        Assert.Contains("<input", html);
        Assert.Contains("checked", html);
    }

    [Fact]
    public void Parse_Strikethrough_RendersDel()
    {
        var (_, html) = _parser.Parse("这是 ~~删除~~ 的内容");

        Assert.Contains("<del>删除</del>", html);
    }

    [Fact]
    public void Parse_MermaidFence_KeepsLanguageClass()
    {
        var md = "```mermaid\nmindmap\n  root((PicoSite))\n    SSG\n```";

        var (_, html) = _parser.Parse(md);

        Assert.Contains("language-mermaid", html);
        Assert.Contains("mindmap", html);
    }

    [Fact]
    public void Parse_EmojiShortcut_RendersEmoji()
    {
        var (_, html) = _parser.Parse("欢迎 :smile:");

        Assert.Contains("😄", html);
    }
}
