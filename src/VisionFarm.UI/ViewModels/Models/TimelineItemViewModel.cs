namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 时间线 ViewModel。
/// </summary>
public partial class TimelineItemViewModel : ObservableObject
{
    /// <summary>
    /// 节点时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 时间格式
    /// </summary>
    public string? TimeFormat { get; set; }

    /// <summary>
    /// 节点描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 节点头
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// 节点类型
    /// </summary>
    public TimelineItemType ItemType { get; set; }
}
