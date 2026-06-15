using OpenCvSharp;
using SkiaSharp;

namespace VisionFarm.Runtime.Renderers;

/// <summary>
/// 视频帧缓冲区（双缓冲）
/// </summary>
/// <param name="width">帧宽度</param>
/// <param name="height">帧高度</param>
public sealed class VideoFrameBuffer(int width, int height)
{
    private readonly Mat _bgra = new(height, width, MatType.CV_8UC4);

    private SKBitmap _front = CreateBitmap(width, height);
    private SKBitmap _back = CreateBitmap(width, height);

    private readonly Lock _lock = new();

    public int Width { get; } = width;

    public int Height { get; } = height;

    public event Action? FrameUpdated;

    public SKBitmap FrontBitmap
    {
        get
        {
            lock (_lock)
            {
                return _front;
            }
        }
    }

    private static SKBitmap CreateBitmap(int w, int h)
        => new(new SKImageInfo(
            w,
            h,
            SKColorType.Bgra8888,
            SKAlphaType.Premul));

    public void Write(Mat frame)
    {
        if (frame.Empty())
        {
            return;
        }

        Mat src;

        if (frame.Type() == MatType.CV_8UC4)
        {
            src = frame;
        }
        else
        {
            // 转换为 BGRA 类型
            Cv2.CvtColor(
                frame,
                _bgra,
                ColorConversionCodes.BGR2BGRA);

            src = _bgra;
        }

        var bytes = src.Step() * src.Rows;

        unsafe
        {
            // 深度拷贝
            Buffer.MemoryCopy(
                src.Data.ToPointer(),
                _back.GetPixels().ToPointer(),
                bytes,
                bytes);
        }

        // 加锁，保证
        lock (_lock)
        {
            (_front, _back) = (_back, _front);
        }

        FrameUpdated?.Invoke();
    }
}
