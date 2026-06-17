using OpenCvSharp;
using FFmpeg.AutoGen.Abstractions;
using Size = System.Drawing.Size;

namespace VisionIndustry.Runtime.FFmpeg;

/// <summary>
/// 像素格式转换器
/// </summary>
internal sealed unsafe class VideoFrameConverter : IDisposable
{
    private SwsContext* _swsContext;

    private readonly Lock _lock = new();

    private readonly AVFramePool _framePool;

    private readonly AVPixelFormat _dstPixelFormat;

    private int _srcWidth;
    private int _srcHeight;
    private AVPixelFormat _srcPixelFormat;

    private readonly int _dstWidth;
    private readonly int _dstHeight;

    private bool _disposed;

    public VideoFrameConverter(
        Size sourceSize,
        AVPixelFormat sourcePixelFormat,
        Size destinationSize,
        AVPixelFormat destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24)
    {
        _srcWidth = sourceSize.Width;
        _srcHeight = sourceSize.Height;
        _srcPixelFormat = sourcePixelFormat;

        _dstWidth = destinationSize.Width;
        _dstHeight = destinationSize.Height;

        _dstPixelFormat = destinationPixelFormat;

        CreateContext();

        _framePool = new AVFramePool(
            _dstWidth,
            _dstHeight,
            destinationPixelFormat,
            16);
    }

    private void CreateContext()
    {
        // 初始化图像缩放与像素格式转换的上下文
        _swsContext = ffmpeg.sws_getContext(
            _srcWidth,
            _srcHeight,
            _srcPixelFormat,                    // srcFormat => 源图像的像素格式
            _dstWidth,
            _dstHeight,
            _dstPixelFormat,                    // dstFormat => 目标图像的像素格式
            (int)SwsFlags.SWS_FAST_BILINEAR,    // flags => 选择缩放算法(只有当源图像和目标图像大小不同时有效),一般选择 SWS_FAST_BILINEAR
            null,                               // *srcFilter => 源图像的滤波器信息, 若不需要传 NULL
            null,                               // *dstFilter => 目标图像的滤波器信息, 若不需要传 NULL
            null);                              // *param => 特定缩放算法需要的参数，默认为 NULL

        if (_swsContext == null)
        {
            throw new InvalidOperationException("Failed to create SwsContext.");
        }
    }

    public AVFrame* Convert(AVFrame* sourceFrame)
    {
        RecreateContextIfNeeded(sourceFrame);

        var dstFrame = _framePool.Rent();

        ffmpeg.av_frame_make_writable(dstFrame)
            .ThrowExceptionIfError();

        // 视频像素格式和分辨率的转换（例如，将源 1920x1080 YUV420P 转换为 640x640 BGR24）
        ffmpeg.sws_scale(
            _swsContext,
            sourceFrame->data,          // srcSlice[] => 源图像的每个颜色通道的数据指针
            sourceFrame->linesize,      // srcStride[] => 源图像的每个颜色通道的跨度
            0,                          // srcSliceY => 起始位置
            sourceFrame->height,        // srcSliceH => 处理多少行。若 srcSliceY=0，srcSliceH=height，表示一次性处理完整个图像
            dstFrame->data,            // dst[] => 每个颜色通道数据指针（定义目标图像信息）
            dstFrame->linesize);       // dstStride[] => 每个颜色通道行字节数（定义目标图像信息）

        dstFrame->pts = sourceFrame->pts;

        return dstFrame;
    }

    private void RecreateContextIfNeeded(AVFrame* sourceFrame)
    {
        if (sourceFrame->width == _srcWidth &&
            sourceFrame->height == _srcHeight &&
            sourceFrame->format == (int)_srcPixelFormat)
        {
            return;
        }

        lock (_lock)
        {
            if (_swsContext != null)
            {
                ffmpeg.sws_freeContext(_swsContext);
            }

            _srcWidth = sourceFrame->width;
            _srcHeight = sourceFrame->height;
            _srcPixelFormat = (AVPixelFormat)sourceFrame->format;

            CreateContext();
        }
    }

    public void ReturnFrame(AVFrame* frame)
    {
        _framePool.Return(frame);
    }

    public Mat ConvertToMat(AVFrame* sourceFrame)
    {
        var frame = Convert(sourceFrame);

        try
        {
            return Mat.FromPixelData(
                    frame->height,
                    frame->width,
                    MatType.CV_8UC3,
                    (IntPtr)frame->data[0],
                    frame->linesize[0])
                .Clone();
        }
        finally
        {
            ReturnFrame(frame);
        }
    }

    public Mat ConvertToMatUnsafe(AVFrame* sourceFrame, out AVFrame* frame)
    {
        frame = Convert(sourceFrame);

        return Mat.FromPixelData(
            frame->height,
            frame->width,
            MatType.CV_8UC3, // 可以做对应转换
            (IntPtr)frame->data[0],
            frame->linesize[0]);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;

        _framePool.Dispose();

        if (_swsContext != null)
        {
            ffmpeg.sws_freeContext(_swsContext);
            _swsContext = null;
        }
    }
}
