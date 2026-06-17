using FFmpeg.AutoGen.Abstractions;
using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;
using OpenCvSharp;
using SkiaSharp;
using VisionIndustry.Runtime.FFmpeg;
using Size = System.Drawing.Size;

namespace VisionIndustry.Runtime.Video;

/// <summary>
/// 基于 FFmpeg 的视频采集。
/// </summary>
internal sealed class FFmpegVideoSource : IVideoSource
{
    public event Action<Mat>? FrameReceived;

    public Task StartAsync()
    {
        FrameReceived += VideoSourceFrameReceived;

        FFmpegBinariesHelper.RegisterFFmpegBinaries();

        DynamicallyLoadedBindings.Initialize();

        ConfigureHWDecoder(out var deviceType);

        DecodeAllFramesToImages(deviceType);

        EncodeImagesToH264();

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        FrameReceived -= VideoSourceFrameReceived;
        return Task.CompletedTask;
    }

    private void VideoSourceFrameReceived(Mat obj)
    {
    }

    private static void ConfigureHWDecoder(out AVHWDeviceType HWtype)
    {
        HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        Console.WriteLine("Use hardware acceleration for decoding?[n]");
        var key = Console.ReadLine();
        var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();

        if (key == "y")
        {
            Console.WriteLine("Select hardware decoder:");
            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var number = 0;

            while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                Console.WriteLine($"{++number}. {type}");
                availableHWDecoders.Add(number, type);
            }

            if (availableHWDecoders.Count == 0)
            {
                Console.WriteLine("Your system have no hardware decoders.");
                HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
                return;
            }

            var decoderNumber = availableHWDecoders
                .SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
            if (decoderNumber == 0)
            {
                decoderNumber = availableHWDecoders.First().Key;
            }

            Console.WriteLine($"Selected [{decoderNumber}]");
            _ = int.TryParse(Console.ReadLine(), out var inputDecoderNumber);
            availableHWDecoders.TryGetValue(inputDecoderNumber == 0 ? decoderNumber : inputDecoderNumber,
                out HWtype);
        }
    }

    private static unsafe void DecodeAllFramesToImages(AVHWDeviceType HWDevice)
    {
        // decode all frames from url, please not it might local resorce, e.g. string url = "../../sample_mpeg4.mp4";

        var url = "https://test-videos.co.uk/vids/bigbuckbunny/mp4/h264/1080/Big_Buck_Bunny_1080_10s_1MB.mp4"; // ~1 MB, 10 s, 1080p H.264
        using var vsd = new VideoStreamDecoder(url, HWDevice);

        Console.WriteLine($"codec name: {vsd.CodecName}");

        var info = vsd.GetMetadata();
        info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

        var sourceSize = vsd.FrameSize;
        var sourcePixelFormat = HWDevice == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
            ? vsd.PixelFormat
            : GetHWPixelFormat(HWDevice);
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.@AV_PIX_FMT_BGRA;
        using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

        var frameNumber = 0;

        while (vsd.TryDecodeNextFrame(out var frame))
        {
            var convertedFrame = vfc.Convert(frame);
            WriteFrame(convertedFrame, frameNumber);

            Console.WriteLine($"frame: {frameNumber}");
            frameNumber++;
            if (frameNumber > 1000)
            {
                break;
            }
        }
    }

    private static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
    {
        return hWDevice switch
        {
            AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
            AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
            AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
            AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
            AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
            _ => AVPixelFormat.AV_PIX_FMT_NONE
        };
    }

    private static unsafe void EncodeImagesToH264()
    {
        var frameFiles = Directory.GetFiles("./frames", "frame.*.jpg").OrderBy(x => x).ToArray();
        using var fistFrameImage = ReadFrame(frameFiles.First());

        var outputFileName = "frames/out.h264";
        var fps = 25;
        var sourceSize = new Size(fistFrameImage.Width, fistFrameImage.Height);
        var sourcePixelFormat = AVPixelFormat.@AV_PIX_FMT_BGRA;
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;
        using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

        using var fs = File.Open(outputFileName, FileMode.Create);

        using var vse = new H264VideoStreamEncoder(fs, fps, destinationSize);

        var frameNumber = 0;

        foreach (var frameFile in frameFiles)
        {
            using var bitmap = ReadFrame(frameFile);
            var bitmapData = bitmap.Bytes;
            fixed (byte* pBitmapData = bitmapData)
            {
                var data = new byte_ptr8 { [0] = pBitmapData };
                var linesize = new int8 { [0] = bitmapData.Length / sourceSize.Height };
                var frame = new AVFrame
                {
                    data = data,
                    linesize = linesize,
                    height = sourceSize.Height
                };
                var convertedFrame = vfc.Convert(&frame);
                convertedFrame->pts = frameNumber * fps;
                vse.Encode(*convertedFrame);
            }

            Console.WriteLine($"frame: {frameNumber}");
            frameNumber++;
        }

        vse.Drain();
    }

    private static unsafe void WriteFrame(AVFrame* convertedFrame, int frameNumber)
    {
        var imageInfo = new SKImageInfo(convertedFrame->width, convertedFrame->height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var bitmap = new SKBitmap();
        bitmap.InstallPixels(imageInfo, (IntPtr)convertedFrame->data[0]);
        using var stream = File.Create($"frames/frame.{frameNumber:D8}.jpg");
        bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
    }

    private static SKBitmap ReadFrame(string frameFile)
    {
        using var codec = SKCodec.Create(frameFile);
        return SKBitmap.Decode(codec);
    }
}
