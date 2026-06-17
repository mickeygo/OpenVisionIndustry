using FFmpeg.AutoGen.Abstractions;

namespace VisionIndustry.Runtime.FFmpeg;

internal sealed unsafe class LatestFrameBuffer
{
    private AVFrame* _latestFrame;

    private readonly Lock _lock = new();

    public void Update(AVFrame* frame, VideoFrameConverter converter)
    {
        lock (_lock)
        {
            if (_latestFrame != null)
            {
                converter.ReturnFrame(_latestFrame);
            }

            _latestFrame = frame;
        }
    }

    public AVFrame* GetLatest()
    {
        lock (_lock)
        {
            return _latestFrame;
        }
    }
}
