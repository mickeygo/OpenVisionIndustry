using System.Runtime.InteropServices;
using FFmpeg.AutoGen.Abstractions;

namespace VisionIndustry.Runtime.FFmpeg;

internal static class FFmpegHelper
{
    public static int ThrowExceptionIfError(this int error)
    {
        if (error < 0)
        {
            throw new FFmpegException(error, av_strerror(error));
        }

        return error;
    }

    public static unsafe string? av_strerror(int error)
    {
        var bufferSize = 1024;
        var buffer = stackalloc byte[bufferSize];
        ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message;
    }
}
