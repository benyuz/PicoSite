namespace PicoSite.Models;

public class SiteModel
{
    public string Title { get; set; } = "PicoSite";
    public string? Description { get; set; }
    public List<PageModel> Pages { get; set; } = new();

    /// <summary>当前语言代码（如 \"zh\"）；非语言页面为 null。</summary>
    public string? Language { get; set; }

    /// <summary>站点所有可用语言代码列表。</summary>
    public List<string> Languages { get; set; } = new();

    /// <summary>默认语言代码。</summary>
    public string DefaultLanguage { get; set; } = "";

    /// <summary>站点部署子路径（如 "/PicoSite/"），为空时链接为根路径。</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>GitHub 仓库链接（header 社交链接区用）。</summary>
    public string? Github { get; set; }

    /// <summary>联系邮箱（header 社交链接区用）。</summary>
    public string? Email { get; set; }

    /// <summary>自定义变量：picosite.json / site.json 中未声明的字段，
    /// 模板中通过 {{ site.xxx }} 访问。</summary>
    public Dictionary<string, object> Variables { get; set; } = new();
}
