using OpenCvSharp;
using SkiaSharp;

namespace VisionFarm.Runtime.Converters;

/// <summary>
/// 转换器，用于将 <see cref="Mat"/> 转换为 <see cref="SKBitmap"/>。
/// </summary>
/// <param name="sKImageInfo">要创建的 SKBitmap 信息。</param>
public sealed class MatToSKBitmapConverter(SKImageInfo sKImageInfo) : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// 获取转换后的 SKBitmap。
    /// </summary>
    public SKBitmap Bitmap { get; } = new(sKImageInfo);

    /// <summary>
    /// 转换器，用于将 <see cref="Mat"/> 转换为 <see cref="SKBitmap"/>。
    /// </summary>
    /// <param name="width">要创建的 SKBitmap 宽度。</param>
    /// <param name="height">要创建的 SKBitmap 高度。</param>
    public MatToSKBitmapConverter(int width, int height)
        : this(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul))
    {
    }

    /// <summary>
    /// 转换为 <see cref="SKBitmap"/>。
    /// </summary>
    /// <param name="mat">要转换的 mat</param>
    /// <param name="autoDispose">是否自动释放 mat</param>
    /// <returns></returns>
    public void Convert(Mat mat, bool autoDispose = false)
    {
        /*
        * OpenCV 摄像头、FFmpeg 解码 后的格式通常为 BGR（CV_8UC3），Avalonia WriteableBitmap 最常用的格式是 BGRA（PixelFormat.Bgra8888），需要做转换
        */

        var bgraMat = mat;

        // 是否需要转换为 BGRA 类型
        var shouldCvt2Bgra = mat.Type() != MatType.CV_8UC4;
        if (shouldCvt2Bgra)
        {
            // 转换为 BGRA 类型（深拷贝）
            bgraMat = mat.CvtColor(ColorConversionCodes.BGR2BGRA);
        }

        var bytes = bgraMat.Step() * bgraMat.Rows;

        unsafe
        {
            // 深度拷贝
            Buffer.MemoryCopy(
                bgraMat.Data.ToPointer(),
                Bitmap.GetPixels().ToPointer(),
                bytes,
                bytes);
        }

        // 深度拷贝，可以对源 Mat 进行释放。
        if (shouldCvt2Bgra)
        {
            bgraMat.Dispose();
        }
        else if (autoDispose)
        {
            mat.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Bitmap.Dispose();
    }
}
