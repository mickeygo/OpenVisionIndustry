using System.Runtime.InteropServices;
using FFmpeg.AutoGen.Abstractions;
using Size = System.Drawing.Size;

namespace VisionIndustry.Runtime.FFmpeg;

/// <summary>
/// 视频解码器。
/// </summary>
internal sealed unsafe class VideoStreamDecoder : IDisposable
{
    private AVFormatContext* _formatContext; // 媒体容器上下文
    private AVCodecContext* _codecContext; // 解码器上下文
    private AVPacket* _packet; // 压缩数据包
    private AVFrame* _frame;
    private AVFrame* _swFrame;

    private readonly string _url;

    private int _streamIndex;
    private bool _flushed;
    private bool _disposed;

    /// <summary>
    /// 视音频编码格式名称
    /// </summary>
    public string CodecName { get; private set; } = string.Empty;

    /// <summary>
    /// 帧尺寸
    /// </summary>
    public Size FrameSize { get; private set; }

    /// <summary>
    /// 像素格式
    /// </summary>
    public AVPixelFormat PixelFormat { get; private set; }

    /// <summary>
    /// 是否为硬件解码器
    /// </summary>
    public bool IsHardwareDecoder => _codecContext != null && _codecContext->hw_device_ctx != null;

    /// <summary>
    /// 初始化一个新的 <see cref="VideoStreamDecoder"/> 对象。
    /// </summary>
    /// <param name="url">
    /// 媒体源，可以为本地文件、网络流（rtsp://、rtmp://、http://）等。
    /// </param>
    /// <param name="hwDeviceType"></param>
    public VideoStreamDecoder(string url, AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
    {
        _url = url;

        OpenStream(hwDeviceType);

        /*
         * 视频硬解码流程：
         *  打开输入文件 -> avformat_open_input()
         *  读取媒体文件信息 -> avformat_find_stream_info()
         *  查询视频流 ID -> av_find_best_stream()
         *  获取解码器 -> avcodec_find_decoder()
         *  获取解码上下文 -> avcodec_alloc_context3()
         *  为解码器上下文赋值 -> avcodec_parameters_to_context()
         *  [检索解码器支持的硬件配置] -> avcodec_get_hw_config()
         *  [遍历支持的硬件解码器设备类型] -> av_hwdevice_iterate_types()
         *  [打开指定类型的硬件编码器设备并创建上下文] -> av_hwdevice_ctx_create()
         *  [注册用于获取硬件像素格式的回调函数] -> AVCodecContext->get_format
         *  打开解码器 -> avcodec_open2()
         *  读取原始数据帧 -> av_read_frame()
         *  将数据发给解码器 -> avcodec_send_packet()
         *  从解码器读取解码后的数据 -> avcodec_receive_frame()
         *  [将解码后的数据从 GPU 拷贝到 CPU] -> av_hwframe_transfer_data()/av_hwframe_map()
         */
    }

    private void OpenStream(AVHWDeviceType hwDeviceType)
    {
        _formatContext = ffmpeg.avformat_alloc_context(); // 创建 FormatContext，用于打开媒体

        AVDictionary* options = null;

        /* RTSP 低延迟参数 */
        ffmpeg.av_dict_set(&options, "rtsp_transport", "tcp", 0);
        ffmpeg.av_dict_set(&options, "fflags", "nobuffer", 0);
        ffmpeg.av_dict_set(&options, "flags", "low_delay", 0);
        ffmpeg.av_dict_set(&options, "max_delay", "500000", 0);
        ffmpeg.av_dict_set(&options, "stimeout", "5000000", 0);

        var formatContext = _formatContext;

        // 打开媒体源
        ffmpeg.avformat_open_input(
                &formatContext,
                _url,
                null,
                &options)
            .ThrowExceptionIfError();

        ffmpeg.av_dict_free(&options);
        ffmpeg.avformat_find_stream_info(_formatContext, null).ThrowExceptionIfError();

        // 找最佳视频流
        AVCodec* codec = null;
        _streamIndex =
            ffmpeg.av_find_best_stream(
                    _formatContext,
                    AVMediaType.AVMEDIA_TYPE_VIDEO,
                    -1,
                    -1,
                    &codec,
                    0)
                .ThrowExceptionIfError();

        // 创建 CodecContext，创建解码器上下文
        _codecContext = ffmpeg.avcodec_alloc_context3(codec);

        ffmpeg.avcodec_parameters_to_context(
                _codecContext,
                _formatContext->streams[_streamIndex]->codecpar)
            .ThrowExceptionIfError();

        // 多线程解码
        _codecContext->thread_count = Environment.ProcessorCount;
        _codecContext->thread_type = ffmpeg.FF_THREAD_FRAME;

        if (hwDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            try
            {
                // 硬件解码
                ffmpeg.av_hwdevice_ctx_create(
                        &_codecContext->hw_device_ctx,
                        hwDeviceType,
                        null,
                        null,
                        0)
                    .ThrowExceptionIfError();
            }
            catch
            {
                // 自动回退软件解码
            }
        }

        // 打开解码器，真正初始化解码器
        ffmpeg.avcodec_open2(_codecContext, codec, null).ThrowExceptionIfError();

        CodecName = ffmpeg.avcodec_get_name(codec->id);

        FrameSize = new Size(_codecContext->width, _codecContext->height);

        PixelFormat = _codecContext->pix_fmt;

        _packet = ffmpeg.av_packet_alloc();
        _frame = ffmpeg.av_frame_alloc();
        _swFrame = ffmpeg.av_frame_alloc();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool TryDecodeNextFrame([MaybeNullWhen(false)] out AVFrame* frame)
    {
        /*  
         *  EOF 时 Flush Decoder
         *  支持 B Frame 尾帧输出 
         *  避免重复 Flush
         */

        frame = null;

        while (true)
        {
            // 优先从解码器取帧
            var ret = ffmpeg.avcodec_receive_frame(_codecContext, _frame);

            if (ret == 0)
            {
                if (IsHardwareDecoder)
                {
                    // 使用硬件解码，将解码后的数据从GPU拷贝到CPU中。
                    // 可以用 av_hwframe_map() 进行性能优化
                    ffmpeg.av_hwframe_transfer_data(
                            _swFrame,
                            _frame,
                            0)
                        .ThrowExceptionIfError();

                    frame = _swFrame;
                }
                else
                {
                    frame = _frame;
                }

                return true;
            }

            // 解码器已经完全结束
            if (ret == ffmpeg.AVERROR_EOF)
            {
                return false;
            }

            // 除 EAGAIN 外都是错误
            // EAGAIN 表示数据不足以组成完整的 frame（解码器会缓存数据），需要继续发送更多 packet
            if (ret != ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                ret.ThrowExceptionIfError();
            }

            // EAGAIN：继续送 Packet
            if (_flushed)
            {
                return false;
            }

            while (true)
            {
                ffmpeg.av_packet_unref(_packet);

                // 读取压缩数据包
                ret = ffmpeg.av_read_frame(_formatContext, _packet);

                // 文件结束
                if (ret == ffmpeg.AVERROR_EOF)
                {
                    // Flush Decoder
                    ffmpeg.avcodec_send_packet(_codecContext, null).ThrowExceptionIfError();

                    _flushed = true;

                    break;
                }

                ret.ThrowExceptionIfError();

                if (_packet->stream_index != _streamIndex)
                {
                    continue;
                }

                // 发送数据包给解码器
                ffmpeg.avcodec_send_packet(_codecContext, _packet).ThrowExceptionIfError();

                break;
            }
        }
    }

    /// <summary>
    /// RTSP断线重连
    /// </summary>
    public bool Reconnect(AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
    {
        try
        {
            DisposeInternal();

            OpenStream(hwDeviceType);

            _flushed = false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取视频的元数据，例如 作者、版权、描述等。
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string> GetMetadata()
    {
        var result = new Dictionary<string, string>();

        AVDictionaryEntry* tag = null;

        while ((tag =
                    ffmpeg.av_dict_get(
                        _formatContext->metadata,
                        string.Empty,
                        tag,
                        ffmpeg.AV_DICT_IGNORE_SUFFIX))
               != null)
        {
            result[
                Marshal.PtrToStringAnsi(
                    (nint)tag->key
                ) ?? string.Empty
            ] =
                Marshal.PtrToStringAnsi(
                    (nint)tag->value
                ) ?? string.Empty;
        }

        return result;
    }

    private void DisposeInternal()
    {
        if (_frame != null)
        {
            var p = _frame;
            ffmpeg.av_frame_free(&p);
            _frame = null;
        }

        if (_swFrame != null)
        {
            var p = _swFrame;
            ffmpeg.av_frame_free(&p);
            _swFrame = null;
        }

        if (_packet != null)
        {
            var p = _packet;
            ffmpeg.av_packet_free(&p);
            _packet = null;
        }

        if (_codecContext != null)
        {
            var p = _codecContext;
            ffmpeg.avcodec_free_context(&p);
            _codecContext = null;
        }

        if (_formatContext != null)
        {
            var p = _formatContext;
            ffmpeg.avformat_close_input(&p);
            _formatContext = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        DisposeInternal();

        GC.SuppressFinalize(this);
    }
}
