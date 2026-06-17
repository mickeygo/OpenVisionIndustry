using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FFmpeg.AutoGen.Abstractions;

namespace VisionIndustry.Runtime.FFmpeg;

/// <summary>
/// 创建 WriteableBitmap 对象。
/// </summary>
/// <param name="width">像素（宽）</param>
/// <param name="height">像素（高）</param>
/// <param name="dpi">DPI（每英寸点数），默认为 96</param>
/// <returns></returns>
public sealed class AvaloniaAVFrameRenderer(
    int width,
    int height,
    double dpi = 96)
{
    private readonly WriteableBitmap _bitmap = new(
        new PixelSize(width, height), // 像素
        new Vector(dpi, dpi), // DPI，每英寸点数，表示每英寸拥有的像素个数
        PixelFormat.Bgra8888, // 像素格式
        AlphaFormat.Unpremul); // 调色板

    public WriteableBitmap Bitmap => _bitmap;

    public unsafe void Render(AVFrame* frame)
    {
        using var fb = _bitmap.Lock();

        var src = frame->data[0];
        var dst = (byte*)fb.Address;
        var srcStride = frame->linesize[0];
        var dstStride = fb.RowBytes;
        var copyWidth = Math.Min(srcStride, dstStride);
        var height = _bitmap.PixelSize.Height;

        // 逐行拷贝图像数据
        for (var y = 0; y < height; y++)
        {
            Buffer.MemoryCopy(
                src + y * srcStride,
                dst + y * dstStride,
                copyWidth,
                copyWidth);
        }
    }
}
