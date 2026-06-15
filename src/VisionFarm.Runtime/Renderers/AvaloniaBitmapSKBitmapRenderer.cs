using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace VisionFarm.Runtime.Renderers;

public sealed class AvaloniaBitmapSKBitmapRenderer(PixelSize pixelSize, Vector vector) : IDisposable
{
    private readonly WriteableBitmap _bitmap = new(
        pixelSize,
        vector,
        PixelFormat.Bgra8888,
        AlphaFormat.Premul);

    /// <summary>
    /// 获取 Bitmap。
    /// </summary>
    public Bitmap Bitmap => _bitmap;

    public AvaloniaBitmapSKBitmapRenderer(int width, int height, double dpi = 96)
        : this(new PixelSize(width, height), new Vector(dpi, dpi))
    {
    }

    public unsafe void Update(SKBitmap skBitmap)
    {
        using var fb = _bitmap.Lock();

        var src = (byte*)skBitmap.GetPixels();
        var dst = (byte*)fb.Address;

        var srcStride = skBitmap.RowBytes; // 每一行实际占用多少字节
        var dstStride = fb.RowBytes;

        // 处理 Stride 不一致情况
        var copyBytes = Math.Min(srcStride, dstStride);

        for (var y = 0; y < skBitmap.Height; y++)
        {
            Buffer.MemoryCopy(
                src + y * srcStride,
                dst + y * dstStride,
                copyBytes,
                copyBytes);
        }
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }
}
