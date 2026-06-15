using OpenCvSharp;

namespace VisionFarm.Runtime.Video;

/// <summary>
/// 视频采集接口。
/// </summary>
public interface IVideoSource
{
    /// <summary>
    /// 帧接收事件。
    /// </summary>
    event Action<Mat> FrameReceived;

    /// <summary>
    /// 启动视频采集。
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// 停止视频采集。
    /// </summary>
    /// <returns></returns>
    Task StopAsync();
}
