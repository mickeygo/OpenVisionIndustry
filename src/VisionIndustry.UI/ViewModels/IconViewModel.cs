using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Semi.Avalonia;

namespace VisionIndustry.UI.ViewModels;

/// <summary>
/// Icon 图标 ViewModel。
/// </summary>
public sealed partial class IconViewModel : ViewModelBase
{
    private readonly Dictionary<string, IconItem> _filledIcons = [];
    private readonly Dictionary<string, IconItem> _strokedIcons = [];

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    public ObservableCollection<IconItem> FilteredFilledIcons { get; } = [];

    public ObservableCollection<IconItem> FilteredStrokedIcons { get; } = [];

    // 复制文本到剪贴板。
    [RelayCommand]
    private async Task CopyText(string? text, CancellationToken token)
    {
        await DoSetClipboardTextAsync(text);
    }

    public IconViewModel()
    {
        foreach (var dict in new Icons().MergedDictionaries.OfType<ResourceDictionary>())
        {
            foreach (var key in dict.Keys)
            {
                if (dict[key] is Geometry geometry)
                {
                    var icon = new IconItem
                    {
                        ResourceKey = key.ToString(),
                        Geometry = geometry
                    };

                    var geometryName = key.ToString()!;
                    if (geometryName.EndsWith("Stroked"))
                    {
                        _strokedIcons[geometryName.ToLowerInvariant()] = icon;
                    }
                    else
                    {
                        _filledIcons[geometryName.ToLowerInvariant()] = icon;
                    }
                }
            }
        }

        OnSearchTextChanged(string.Empty);
    }

    partial void OnSearchTextChanged(string? value)
    {
        var search = value?.ToLowerInvariant() ?? string.Empty;

        FilteredFilledIcons.Clear();
        foreach (var pair in _filledIcons.Where(i => i.Key.Contains(search)))
        {
            FilteredFilledIcons.Add(pair.Value);
        }

        FilteredStrokedIcons.Clear();
        foreach (var pair in _strokedIcons.Where(i => i.Key.Contains(search)))
        {
            FilteredStrokedIcons.Add(pair.Value);
        }
    }

    private async Task DoSetClipboardTextAsync(string? text)
    {
        if (TopLevel != null)
        {
            await ClipboardExtensions.SetTextAsync(TopLevel.Clipboard!, text);
            ShowNotification("已复制", text);
        }
        else
        {
            ShowNotification("复制失败", "", Avalonia.Controls.Notifications.NotificationType.Error);
        }
    }
}

public sealed class IconItem
{
    [NotNull]
    public string? ResourceKey { get; set; }

    [NotNull]
    public Geometry? Geometry { get; set; }
}
