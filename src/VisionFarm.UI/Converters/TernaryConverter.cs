using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VisionFarm.UI.Converters;

/// <summary>
/// 三元表达式转换器。
/// </summary>
/// <remarks>
/// 示例1，IsTrue ? "OK" : "NG" ：
/// <code>
/// &lt;RadioButton
///     IsEabled="{Binding IsTrue, Converter={StaticResource TernaryConverter}, ConverterParameter=OK|NG}"
/// /&gt;
/// </code>
/// 示例2，IsTrue ? "OK" : "" ：
/// <code>
/// &lt;RadioButton
///     IsEabled="{Binding IsTrue, Converter={StaticResource TernaryConverter}, ConverterParameter=OK}"
/// /&gt;
/// </code>
/// </remarks>
public sealed class TernaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string str)
        {
            return null;
        }

        var segments = str.Split('|');
        if (value is true && segments.Length > 0)
        {
            return segments[0];
        }

        if (value is false && segments.Length > 1)
        {
            return segments[1];
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
