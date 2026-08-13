namespace PicoSite.Models;

public class SiteConfig
{
    public string? Title { get; set; } = "PicoSite";
    public string? Description { get; set; }
    public string? Theme { get; set; } = "default";
    public int Port { get; set; } = 8090;
    public string? Output { get; set; } = "./_site";

    /// <summary>
    /// 默认语言代码（如 "zh"）。未配置时取第一个检测到的语言目录。
    /// </summary>
    public string? DefaultLanguage { get; set; }

    /// <summary>
    /// 站点部署子路径（如 "/PicoSite/"，GitHub Pages 项目页用）。
    /// 为空时链接为根路径。
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 站点域名（如 "https://benyuz.github.io"，不带末尾斜杠）。
    /// 配置后 sitemap.xml / robots.txt 生成绝对 URL（搜索引擎规范要求）；
    /// 未配置时回退相对路径。
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>GitHub 仓库链接（配置后 header 显示 GitHub 链接）。</summary>
    public string? Github { get; set; }

    /// <summary>联系邮箱（配置后 header 显示邮箱链接）。</summary>
    public string? Email { get; set; }

    /// <summary>自定义变量：picosite.json / site.json 中未声明的字段原样保留，
    /// 通过模板变量 {{ site.xxx }} 访问（JSON 的任意字段均可作为动态变量）。</summary>
    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? Extra { get; set; }
}
