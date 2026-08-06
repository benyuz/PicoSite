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
}
