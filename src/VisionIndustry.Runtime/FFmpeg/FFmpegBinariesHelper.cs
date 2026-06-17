using System.Runtime.InteropServices;
using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;

namespace VisionIndustry.Runtime.FFmpeg;

/// <summary>
/// FFmpeg 二进制
/// </summary>
internal static class FFmpegBinariesHelper
{
    /// <summary>
    /// 注册二进制文件
    /// </summary>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    internal static void RegisterFFmpegBinaries()
    {
        // 设置 ffmepg lib 文件的路径，注意 FFmpeg.AutoGen 主版本号要与 ffmepg 版本对应。
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var current = AppContext.BaseDirectory;
            var probe = Path.Combine("FFmpeg", Environment.Is64BitProcess ? "x64" : "x86");

            var ffmpegBinaryPath = Path.Combine(current, probe);
            if (!Directory.Exists(ffmpegBinaryPath))
            {
                throw new DirectoryNotFoundException(ffmpegBinaryPath);
            }

            DynamicallyLoadedBindings.LibrariesPath = ffmpegBinaryPath;
        }
        else
        {
            throw new NotSupportedException(); // fell free add support for platform of your choose
        }
    }
}
