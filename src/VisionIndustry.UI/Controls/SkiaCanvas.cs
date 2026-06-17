using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace VisionIndustry.UI.Controls;

public sealed class SkiaCanvas : Control
{
    private readonly SkiaDrawOperation _drawOperation = new();

    public static readonly StyledProperty<SKBitmap?> SourceProperty =
        AvaloniaProperty.Register<SkiaCanvas, SKBitmap?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<SkiaCanvas, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<SkiaCanvas, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public static readonly StyledProperty<ulong> FrameVersionProperty =
        AvaloniaProperty.Register<SkiaCanvas, ulong>(nameof(FrameVersion), 0);

    [Content]
    public SKBitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public ulong FrameVersion
    {
        get => GetValue(FrameVersionProperty);
        set => SetValue(FrameVersionProperty, value);
    }

    static SkiaCanvas()
    {
        SourceProperty.Changed.AddClassHandler<SkiaCanvas>(
            static (x, e) =>
            {
                x._drawOperation.ClearBitmap();
            });

        // 属性发生后，重新绘制（执行 Render() 方法）
        AffectsRender<SkiaCanvas>(SourceProperty, StretchProperty, StretchDirectionProperty, FrameVersionProperty);

        // 属性变化后，重新测量控件尺寸
        AffectsMeasure<SkiaCanvas>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // 窗口最小化时不绘制
        if (!IsVisible)
        {
            return;
        }

        var bitmap = Source;
        if (bitmap == null)
        {
            return;
        }

        var bounds =
           new Rect(
               0,
               0,
               Bounds.Width > 0 ? Bounds.Width : bitmap.Width,
               Bounds.Height > 0 ? Bounds.Height : bitmap.Height);
        _drawOperation.Update(bitmap, bounds, Stretch, StretchDirection, FrameVersion);

        context.Custom(_drawOperation);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Source is { } bitmap)
        {
            var size = new Size(bitmap.Width, bitmap.Height);
            return Stretch.CalculateSize(availableSize, size, StretchDirection);
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    protected override AutomationPeer OnCreateAutomationPeer() => new ImageAutomationPeer(this);

    private sealed class SkiaDrawOperation : ICustomDrawOperation
    {
        private SKBitmap? _bitmap;
        private Stretch _stretch;
        private StretchDirection _stretchDirection;
        private ulong _frameVersion;

        private SKRect _src;
        private SKRect _dst;

        private bool _layoutDirty = true;

        public Rect Bounds { get; private set; }

        public void Update(
            SKBitmap bitmap,
            Rect bounds,
            Stretch stretch,
            StretchDirection stretchDirection,
            ulong frameVersion)
        {
            var layoutChanged =
                !ReferenceEquals(_bitmap, bitmap) ||
                Bounds != bounds ||
                _stretch != stretch ||
                _stretchDirection != stretchDirection ||
                _frameVersion != frameVersion;

            if (layoutChanged)
            {
                _layoutDirty = true;

                _bitmap = bitmap;
                Bounds = bounds;
                _stretch = stretch;
                _stretchDirection = stretchDirection;
                _frameVersion = frameVersion;
            }
        }

        public void ClearBitmap()
        {
            _bitmap = null;
        }

        public void Render(ImmediateDrawingContext context)
        {
            if (_bitmap == null)
            {
                return;
            }

            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
            {
                return;
            }

            if (_layoutDirty)
            {
                UpdateRects();
            }

            using var lease = leaseFeature.Lease();
            lease.SkCanvas.DrawBitmap(_bitmap, _src, _dst);
        }

        private void UpdateRects()
        {
            if (_bitmap == null)
            {
                return;
            }

            var rect = new Rect(Bounds.Size);
            var imageSize = new Size(_bitmap.Width, _bitmap.Height);
            var scale = _stretch.CalculateScaling(Bounds.Size, imageSize, _stretchDirection);
            var scaledSize = imageSize * scale;

            var destRect = rect.CenterRect(new Rect(scaledSize)).Intersect(rect);
            var sourceRect = new Rect(imageSize).CenterRect(new Rect(destRect.Size / scale));

            _src = new SKRect(
                (float)sourceRect.Left,
                (float)sourceRect.Top,
                (float)sourceRect.Right,
                (float)sourceRect.Bottom);

            _dst = new SKRect(
                (float)destRect.Left,
                (float)destRect.Top,
                (float)destRect.Right,
                (float)destRect.Bottom);

            _layoutDirty = false;
        }

        public bool HitTest(Point p) => Bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is not SkiaDrawOperation op)
            {
                return false;
            }

            return ReferenceEquals(_bitmap, op._bitmap) &&
                Bounds == op.Bounds &&
                _stretch == op._stretch &&
                _stretchDirection == op._stretchDirection &&
                _src == op._src &&
                _dst == op._dst &&
                _frameVersion == op._frameVersion;
        }

        public void Dispose() { }
    }
}
