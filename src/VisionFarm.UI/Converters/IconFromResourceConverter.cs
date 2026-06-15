using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace VisionFarm.UI.Converters;

/// <summary>
/// Icon 转换器，通过资源文件找到对应的 Icon。
/// </summary>
/// <remarks>
/// Avalonia UI Fluent Icons 提供了一系列 icon 集合，可参考：http://avaloniaui.github.io/icons.html。
/// </remarks>
public sealed class IconFromResourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && Application.Current!.TryFindResource(s, out var result) && result is Geometry)
        {
            return result;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
