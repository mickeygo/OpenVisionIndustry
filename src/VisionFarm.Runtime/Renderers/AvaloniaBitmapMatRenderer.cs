using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OpenCvSharp;

namespace VisionFarm.Runtime.Renderers;

public sealed class AvaloniaBitmapMatRenderer(PixelSize pixelSize, Vector vector) : IDisposable
{
    private readonly WriteableBitmap _bitmap = new(
        pixelSize,
        vector,
        PixelFormat.Bgra8888,
        AlphaFormat.Premul);

    private readonly Mat _bgraFrame = new(); // BGRA 帧

    /// <summary>
    /// 获取 Bitmap。
    /// </summary>
    public Bitmap Bitmap => _bitmap;

    public AvaloniaBitmapMatRenderer(int width, int height, double dpi = 96)
        : this(new PixelSize(width, height), new Vector(dpi, dpi))
    {
    }

    public unsafe void Update(Mat frame)
    {
        var srcFrame = frame;

        // 空帧
        if (srcFrame.Empty())
        {
            return;
        }

        /*
         * OpenCV 摄像头、FFmpeg 解码 后的格式通常为 BGR（CV_8UC3），Avalonia WriteableBitmap 最常用的格式是 BGRA（PixelFormat.Bgra8888），需要做转换
         */

        // 将 BGR（CV_8UC3）转换为 BGRA（CV_8UC4）
        if (frame.Type() != MatType.CV_8UC4)
        {
            Cv2.CvtColor(
                frame,
                _bgraFrame,
                ColorConversionCodes.BGR2BGRA);

            srcFrame = _bgraFrame;
        }

        using var fb = _bitmap.Lock();

        /*
         * new Mat(1080, 1920, MatType.CV_8UC3);
         * 即：宽度 = 1920；高度 = 1080；通道 = 3(BGR)；每通道 = 1字节（0~255）
         * 理论上一行大小：1920 × 3 × 1 = 5760 Bytes；
         * MatType.CV_8UC4 即 通道 = 4(BGRA)，一行大小为 7680 Bytes。
         */

        var srcStride = srcFrame.Step(); // 每一行实际占用多少字节
        var dstStride = fb.RowBytes; // 同上

        if (srcFrame.IsContinuous() && srcStride == dstStride)
        {
            // 整块拷贝（高性能）
            var bytes = srcStride * srcFrame.Rows;
            Buffer.MemoryCopy(
                srcFrame.Data.ToPointer(),
                fb.Address.ToPointer(),
                bytes,
                bytes);
        }
        else
        {
            // 逐行拷贝
            var src = (byte*)srcFrame.Data;
            var dst = (byte*)fb.Address;

            var copyBytes = Math.Min(srcStride, dstStride);
            var rows = srcFrame.Rows;

            for (var y = 0; y < rows; y++)
            {
                Buffer.MemoryCopy(
                    src + y * srcStride,
                    dst + y * dstStride,
                    copyBytes,
                    copyBytes);
            }
        }
    }

    public void Dispose()
    {
        _bgraFrame.Dispose();
        _bitmap.Dispose();
    }
}
