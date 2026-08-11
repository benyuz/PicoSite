namespace PicoSite.Models;

public class PageModel
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public DateTime? Date { get; set; }
    public string? Excerpt { get; set; }
    public string Content { get; set; } = "";
    public bool HasContentH1 { get; set; }
    public string SourcePath { get; set; } = "";
    public Dictionary<string, object>? FrontMatter { get; set; }
}
