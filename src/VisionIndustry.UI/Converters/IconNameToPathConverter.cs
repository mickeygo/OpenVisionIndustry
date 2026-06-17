using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace VisionIndustry.UI.Converters;

/// <summary>
/// Icon 转换器，通过 icon 名称找到 icon path。
/// </summary>
/// <remarks>
/// Avalonia UI Fluent Icons 提供了一系列 icon 集合，可参考：https://avaloniaui.github.io/icons.html。
/// </remarks>
public sealed class IconNameToPathConverter : IValueConverter
{
    private static readonly Dictionary<string, string> s_paths = [];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && s_paths.TryGetValue(s, out var path))
        {
            return StreamGeometry.Parse(path);
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
