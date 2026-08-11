namespace PicoSite.Models;

/// <summary>
/// 导航渲染树节点。目录节点 Url 为 null，页面节点 Url 为完整路径。
/// </summary>
public class NavNode
{
    public string Title { get; set; } = "";
    public string? Url { get; set; }
    public DateTime? Date { get; set; }
    public string Description { get; set; } = "";
    public List<NavNode> Children { get; set; } = new();
}
