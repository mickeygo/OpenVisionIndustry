namespace VisionFarm.Runtime.FFmpeg;

/// <summary>
/// FFmpeg 异常。
/// </summary>
public class FFmpegException(int error, string? message) : Exception(message)
{
    /// <summary>
    /// 异常代码。
    /// </summary>
    public int Error { get; } = error;
}
