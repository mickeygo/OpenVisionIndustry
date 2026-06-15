using OpenCvSharp;
using SkiaSharp;
using VisionFarm.Runtime.Converters;
using VisionFarm.Runtime.Renderers;
using YoloDotNet;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Extensions;
using YoloDotNet.Models;

namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 主工作台
/// </summary>
internal sealed partial class WorkbenchViewModel : ViewModelBase
{
    private readonly Yolo _yolo = new(new YoloOptions
    {
        ExecutionProvider = new CpuExecutionProvider(@"E:\TestCases\Yolo-Models\yolov8s.onnx"),
    });

    [ObservableProperty]
    public partial SKBitmap? VideoImage { get; set; }

    [ObservableProperty]
    public partial ulong VideoImageVersion { get; set; }

    [ObservableProperty]
    public partial SKBitmap? VideoFrame { get; set; }

    [ObservableProperty]
    public partial ulong VideoFrameVersion { get; set; }

    [ObservableProperty]
    public partial SKBitmap? DetectionFrame { get; set; }

    [RelayCommand]
    private async Task Play()
    {
        // OpenCV 读取视频文件显示
        VideoCapture capture = new(@"E:\TestCases\Videos\vtest.avi");   // 读取本地视频文件
        if (!capture.IsOpened())
        {
            return;
        }

        var interval = TimeSpan.FromMilliseconds(1000.0 / capture.Fps);

        Mat frame = new();
        var sw = Stopwatch.StartNew();

        ulong index = 0;

        //MatToSKBitmapConverter converter = new(capture.FrameWidth, capture.FrameHeight);
        //VideoImage = converter.Bitmap;

        MatToSKBitmapConverter converter2 = new(capture.FrameWidth, capture.FrameHeight);
        VideoFrame = converter2.Bitmap;

        VideoFrameBuffer buffer = new(capture.FrameWidth, capture.FrameHeight);

        while (capture.Read(frame) && !frame.Empty()) // 抓取和解码，返回下一帧
        {
            VideoImage = buffer.FrontBitmap;

            SKBitmap? bitmap = null;
            if (++index % 5 == 0) // 每过指定帧数显示一次
            {
                converter2.Convert(frame);
                bitmap = converter2.Bitmap;
                var result = _yolo.RunObjectDetection(bitmap);
                bitmap.Draw(result);
            }

            //converter.Convert(frame);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                //VideoImageVersion++;

                if (bitmap != null)
                {
                    VideoFrameVersion++;
                    bitmap = null;
                }
            });

            // 计算转换耗费的时间，下一帧显示时会扣掉这个时间
            var delay = interval - sw.Elapsed;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay).ConfigureAwait(false);
            }

            sw.Restart();
        }
    }

    [RelayCommand]
    private async Task ShowDetection()
    {
        var files = Directory.GetFiles(@"E:\TestCases\Images");
        var file = files[Random.Shared.Next(0, files.Length)];

        var bitmap = SKBitmap.Decode(file);
        var result = _yolo.RunObjectDetection(bitmap);
        bitmap.Draw(result);

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            DetectionFrame = bitmap;
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _yolo.Dispose();
    }
}
