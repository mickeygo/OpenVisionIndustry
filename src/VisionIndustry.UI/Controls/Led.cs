using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace VisionIndustry.UI.Controls;

/// <summary>
/// Led 灯状态
/// </summary>
public enum LedState
{
    /// <summary>
    /// 熄灭
    /// </summary>
    Off,

    /// <summary>
    /// 闪烁中
    /// </summary>
    Blinking,

    /// <summary>
    /// Good 状态常亮
    /// </summary>
    Good,

    /// <summary>
    /// Bad 状态常亮
    /// </summary>
    Bad,
}

/// <summary>
/// Led 闪烁灯。
/// </summary>
public class Led : TemplatedControl
{
    /// <summary>
    /// 唯一编号
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    #region 自定义属性

    /// <summary>
    /// LED 的编号。
    /// </summary>
    public int Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public static readonly StyledProperty<int> IndexProperty = AvaloniaProperty.Register<Led, int>(
        name: nameof(Index),
        defaultValue: 0,
        validate: m => m >= 0,
        enableDataValidation: true);

    /// <summary>
    /// LED 的显示文本。
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<Led, string?>(nameof(Text), string.Empty);

    /// <summary>
    /// LED 的显示文本颜色（默认 Black）。
    /// </summary>
    public IBrush? TextColor
    {
        get => GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly StyledProperty<IBrush?> TextColorProperty = AvaloniaProperty.Register<Led, IBrush?>(nameof(TextColor), Brush.Parse("Black"));

    /// <summary>
    /// LED 的显示文本字体尺寸。
    /// </summary>
    public double TextFontSize
    {
        get => GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public static readonly StyledProperty<double> TextFontSizeProperty = AvaloniaProperty.Register<Led, double>(
        name: nameof(TextFontSize),
        defaultValue: 14,
        validate: m => m > 0,
        enableDataValidation: true);

    /// <summary>
    /// LED 分组名。
    /// </summary>
    public string Group
    {
        get => GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    public static readonly StyledProperty<string> GroupProperty = AvaloniaProperty.Register<Led, string>(nameof(Group), string.Empty);

    /// <summary>
    /// LED 的直径。
    /// </summary>
    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public static readonly StyledProperty<double> DiameterProperty = AvaloniaProperty.Register<Led, double>(
        name: nameof(Diameter),
        defaultValue: 36,
        validate: m => m > 0,
        enableDataValidation: true);

    /// <summary>
    /// 熄灭状态颜色（默认 LightGray）
    /// </summary>
    public string OffColor
    {
        get => GetValue(OffColorProperty);
        set => SetValue(OffColorProperty, value);
    }

    public static readonly StyledProperty<string> OffColorProperty = AvaloniaProperty.Register<Led, string>(nameof(OffColor), "LightGray");

    /// <summary>
    /// 闪烁中亮灯状态颜色（默认 Yellow）
    /// </summary>
    public string BlinkingOnColor
    {
        get => GetValue(BlinkingOnColorProperty);
        set => SetValue(BlinkingOnColorProperty, value);
    }

    public static readonly StyledProperty<string> BlinkingOnColorProperty = AvaloniaProperty.Register<Led, string>(nameof(BlinkingOnColor), "Yellow");

    /// <summary>
    /// 闪烁中熄灭状态颜色（默认 DimGray）
    /// </summary>
    public string BlinkingOffColor
    {
        get => GetValue(BlinkingOffColorProperty);
        set => SetValue(BlinkingOffColorProperty, value);
    }

    public static readonly StyledProperty<string> BlinkingOffColorProperty = AvaloniaProperty.Register<Led, string>(nameof(BlinkingOffColor), "DimGray");

    /// <summary>
    /// Good 状态常亮颜色（默认 LimeGreen）
    /// </summary>
    public string GoodColor
    {
        get => GetValue(GoodColorProperty);
        set => SetValue(GoodColorProperty, value);
    }

    public static readonly StyledProperty<string> GoodColorProperty = AvaloniaProperty.Register<Led, string>(nameof(GoodColor), "LimeGreen");

    /// <summary>
    /// Bad 状态常亮颜色（默认 Red）
    /// </summary>
    public string BadColor
    {
        get => GetValue(BadColorProperty);
        set => SetValue(BadColorProperty, value);
    }

    public static readonly StyledProperty<string> BadColorProperty = AvaloniaProperty.Register<Led, string>(nameof(BadColor), "Red");

    /// <summary>
    /// 闪烁时间间隔，默认为 "0:0:1"。
    /// </summary>
    public string BlinkInterval
    {
        get => GetValue(BlinkIntervalProperty);
        set => SetValue(BlinkIntervalProperty, value);
    }

    public static readonly StyledProperty<string> BlinkIntervalProperty = AvaloniaProperty.Register<Led, string>(nameof(BlinkInterval), "0:0:1");

    /// <summary>
    /// 灯状态
    /// </summary>
    public LedState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly StyledProperty<LedState> StateProperty = AvaloniaProperty.Register<Led, LedState>(nameof(State), LedState.Off);

    #endregion

    #region 拖拽属性

    /// <summary>
    /// 控件是否正处于拖拽中。
    /// </summary>
    public bool IsDragging
    {
        get => GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }

    public static readonly StyledProperty<bool> IsDraggingProperty = AvaloniaProperty.Register<Led, bool>(
        nameof(IsDragging),
        defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// 拖拽开始鼠标按下时控件处于的相对位置（控件左上角顶点位置）。
    /// </summary>
    public Point DragMouseDownRelativePosition
    {
        get => GetValue(DragMouseDownRelativePositionProperty);
        set => SetValue(DragMouseDownRelativePositionProperty, value);
    }

    public static readonly StyledProperty<Point> DragMouseDownRelativePositionProperty = AvaloniaProperty.Register<Led, Point>(nameof(DragMouseDownRelativePosition));

    /// <summary>
    /// 拖拽结束鼠标松开时控件处于的相对位置（控件左上角顶点位置）。
    /// </summary>
    public Point DragMouseUpRelativePosition
    {
        get => GetValue(DragMouseUpRelativePositionProperty);
        set => SetValue(DragMouseUpRelativePositionProperty, value);
    }

    public static readonly StyledProperty<Point> DragMouseUpRelativePositionProperty = AvaloniaProperty.Register<Led, Point>(
        nameof(DragMouseUpRelativePosition),
        defaultBindingMode: BindingMode.TwoWay);

    #endregion
}
