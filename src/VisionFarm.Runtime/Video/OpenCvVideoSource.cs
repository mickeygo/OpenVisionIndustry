using OpenCvSharp;

namespace VisionFarm.Runtime.Video;

/// <summary>
/// 基于 OpenCv 的视频采集。
/// </summary>
internal sealed class OpenCvVideoSource : IVideoSource
{
    public event Action<Mat>? FrameReceived;

    public Task StartAsync()
    {
        FrameReceived += VideoSourceFrameReceived;

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
}
