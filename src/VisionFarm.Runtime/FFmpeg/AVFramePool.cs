using System.Collections.Concurrent;
using FFmpeg.AutoGen.Abstractions;

namespace VisionFarm.Runtime.FFmpeg;

/// <summary>
/// AVFrame 池。
/// </summary>
internal sealed unsafe class AVFramePool : IDisposable
{
    private readonly ConcurrentBag<nint> _pool = [];

    private readonly int _width;
    private readonly int _height;
    private readonly AVPixelFormat _pixelFormat;

    private bool _disposed;

    public AVFramePool(
        int width,
        int height,
        AVPixelFormat pixelFormat,
        int initialCount = 8)
    {
        _width = width;
        _height = height;
        _pixelFormat = pixelFormat;

        for (var i = 0; i < initialCount; i++)
        {
            _pool.Add((nint)CreateFrame());
        }
    }

    public AVFrame* Rent()
    {
        if (_pool.TryTake(out var ptr))
        {
            return (AVFrame*)ptr;
        }

        return CreateFrame();
    }

    public void Return(AVFrame* frame)
    {
        if (frame == null)
        {
            return;
        }

        // 解除引用（保留对象），引用计数归零时释放缓冲区内存
        ffmpeg.av_frame_unref(frame);

        _pool.Add((nint)frame);
    }

    private AVFrame* CreateFrame()
    {
        /* 
         * 内存管理流程，完整生命周期示例：
         * 
            AVFrame* frame = av_frame_alloc();  // 仅分配 AVFrame 结构体
                                                // 设置 format/width/height 等参数
            av_frame_get_buffer(frame, 0);      // 分配数据缓冲区

            // 处理帧数据（如解码输出、色彩转换）
            // ...

            av_frame_unref(frame); // 关键：解除引用，引用计数归零时释放缓冲区内存
            av_frame_free(&frame); // 释放 AVFrame 本身
        */

        // 分配一个空的 AVFrame 结构体，但仅会为 AVFrame 结构体本身分配内存（包括其各种字段），并不分配数据缓冲区
        var frame = ffmpeg.av_frame_alloc();

        frame->format = (int)_pixelFormat;
        frame->width = _width;
        frame->height = _height;

        // 为 AVFrame 动态分配数据缓冲区的核心函数，其核心作用是根据已设置的格式、尺寸等参数，自动分配并初始化视频帧或音频帧所需的内存空间，同时 通过引用计数机制管理内存生命周期，避免手动释放导致的泄漏风险。
        // 通过 av_frame_ref 增加计数，av_frame_unref 减少计数，当计数归零时缓冲区会自动释放，无需手动管理。
        // 缓冲区计算，例如：BGR24 1920×1080，计算出缓冲大小 1920×1080×3 约 6MB
        ffmpeg.av_frame_get_buffer(
                frame,
                0)  // 指定缓冲区行数据（linesize）的 字节对齐要求：
                    //  0 表示由 FFmpeg 自动选择当前 CPU 最优对齐方式（强烈推荐），
                    //  非零值（如 32）：强制按指定字节数对齐，仅在特殊场景（如 GPU 交互）需手动控制时使用。
            .ThrowExceptionIfError();

        return frame;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_pool.TryTake(out var ptr))
        {
            var frame = (AVFrame*)ptr;
            ffmpeg.av_frame_free(&frame); // 其内部会调用 av_frame_unref（若 free 之前手动再调用会导致引用计数错误）
        }
    }
}
