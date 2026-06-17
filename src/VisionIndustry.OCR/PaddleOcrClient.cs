using System.Diagnostics;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace VisionIndustry.OCR;

internal sealed class PaddleOcrClient
{
    public void Handle()
    {
        var model = LocalFullModels.ChineseV5;

        using PaddleOcrAll all = new(model, PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = true, // 允许识别有角度的文字
            Enable180Classification = false, // 允许识别旋转角度大于90度的文字
        };

        using var src = Cv2.ImRead(@"temp_plate.png");
        var result = all.Run(src);
        foreach (var region in result.Regions)
        {
            Debug.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:{region.Rect.Size}, Angle: {region.Rect.Angle}");
        }
    }
}
