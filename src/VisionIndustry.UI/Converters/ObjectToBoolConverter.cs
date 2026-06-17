using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VisionIndustry.UI.Converters;

/// <summary>
/// 转换器，如果绑定值与参数相等则为 true，否则为 false。数据类型可以为 字符串、枚举、数值或其他可比较的类型。
/// </summary>
/// <remarks>
/// 示例1，字符串（数值类似）：
/// <code>
/// &lt;RadioButton
///     IsEabled="{Binding MyState, Converter={StaticResource ObjectToBoolConverter}, ConverterParameter=OK}"
/// /&gt;
/// </code>
/// 示例1，枚举：
/// <code>
/// &lt;RadioButton
///     IsEabled="{Binding MyState, Converter={StaticResource ObjectToBoolConverter}, ConverterParameter={x:Static local:MyState.OK}}"
/// /&gt;
/// </code>
/// </remarks>
public sealed class ObjectToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return false;
        }
     
        return value!.Equals(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool v && v == true)
        {
            return parameter;
        }

        return BindingOperations.DoNothing;
    }
}
