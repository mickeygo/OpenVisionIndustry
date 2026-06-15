using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace VisionFarm.UI.Controls;

public sealed class VideoSkiaCanvas : Control
{
    private readonly SkiaDrawOperation _drawOperation = new();

    private DispatcherTimer? _renderTimer;

    public static readonly StyledProperty<SKBitmap?> SourceProperty
        = AvaloniaProperty.Register<VideoSkiaCanvas, SKBitmap?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty
        = AvaloniaProperty.Register<VideoSkiaCanvas, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<VideoSkiaCanvas, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public static readonly StyledProperty<int> IntervalProperty
        = AvaloniaProperty.Register<VideoSkiaCanvas, int>(nameof(Interval), 16); // 16ms，约 60Hz

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

    public int Interval
    {
        get => GetValue(IntervalProperty);
        set
        {
            SetValue(IntervalProperty, value);

            var timer = TimeSpan.FromMilliseconds(value);
            if (_renderTimer != null && _renderTimer.Interval != timer)
            {
                _renderTimer.Interval = timer;
            }
        }
    }

    static VideoSkiaCanvas()
    {
        SourceProperty.Changed.AddClassHandler<VideoSkiaCanvas>(
            static (x, e) =>
            {
                x._drawOperation.ClearBitmap();
            });

        // 属性发生后，重新绘制（执行 Render() 方法）
        AffectsRender<VideoSkiaCanvas>(SourceProperty, StretchProperty, StretchDirectionProperty);

        // 属性变化后，重新测量控件尺寸
        AffectsMeasure<VideoSkiaCanvas>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public void UpdateFrame(SKBitmap frame)
    {
        Source = frame; // 内部会处理 AlphaFormat 和内存
        InvalidateVisual(); // 仅当新帧到达时重绘
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
        _drawOperation.Update(bitmap, bounds, Stretch, StretchDirection);

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Interval)
        };

        _renderTimer.Tick += RenderTimerOnTick;

        _renderTimer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_renderTimer != null)
        {
            _renderTimer.Stop();
            _renderTimer.Tick -= RenderTimerOnTick;
            _renderTimer = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void RenderTimerOnTick(object? sender, EventArgs e)
    {
        if (IsVisible)
        {
            InvalidateVisual();
        }
    }

    private sealed class SkiaDrawOperation : ICustomDrawOperation
    {
        private SKBitmap? _bitmap;
        private Stretch _stretch;
        private StretchDirection _stretchDirection;

        private SKRect _src;
        private SKRect _dst;

        private bool _layoutDirty = true;

        public Rect Bounds { get; private set; }

        public void Update(
            SKBitmap bitmap,
            Rect bounds,
            Stretch stretch,
            StretchDirection stretchDirection)
        {
            var layoutChanged =
               !ReferenceEquals(_bitmap, bitmap) ||
               Bounds != bounds ||
               _stretch != stretch ||
               _stretchDirection != stretchDirection;

            if (layoutChanged)
            {
                _layoutDirty = true;

                _bitmap = bitmap;
                Bounds = bounds;
                _stretch = stretch;
                _stretchDirection = stretchDirection;
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
                _dst == op._dst;
        }

        public void Dispose() { }
    }
}
