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

    /// <summary>GitHub 仓库链接（配置后 header 显示 GitHub 链接）。</summary>
    public string? Github { get; set; }

    /// <summary>联系邮箱（配置后 header 显示邮箱链接）。</summary>
    public string? Email { get; set; }
}
