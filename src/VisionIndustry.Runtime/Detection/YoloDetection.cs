using SkiaSharp;
using YoloDotNet;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Models;

namespace VisionIndustry.Runtime.Detection;

/// <summary>
/// 基于 YOLO 框架的检测实现
/// </summary>
public sealed class YoloDetection : IDisposable
{
    private readonly Yolo _yolo;

    /// <summary>
    /// 初始化一个新的 <see cref="YoloDetection"/> 对象。
    /// </summary>
    /// <param name="model">ONNX 模型</param>
    public YoloDetection(string model)
    {
        _yolo = new(new YoloOptions
        {
            ExecutionProvider = new CpuExecutionProvider(model),
        });
    }

    /// <summary>
    /// 图像分类
    /// </summary>
    /// <param name="img"></param>
    /// <param name="classes"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Classification> RunClassification(SKImage img, int classes = 1)
    {
        return _yolo.RunClassification(img, classes);
    }

    /// <summary>
    /// 图像分类
    /// </summary>
    /// <param name="img"></param>
    /// <param name="classes"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Classification> RunClassification(SKBitmap img, int classes = 1)
    {
        return _yolo.RunClassification(img, classes);
    }

    /// <summary>
    /// 目标检测
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    public IReadOnlyCollection<ObjectDetection> RunObjectDetection(SKImage img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        // For YOLOv10 and YOLOv26, IoU is ignored since post-processing is handled internally by the model.
        return _yolo.RunObjectDetection(img, confidence, iou, roi);
    }

    /// <summary>
    /// 目标检测
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    public IReadOnlyCollection<ObjectDetection> RunObjectDetection(SKBitmap img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        // For YOLOv10 and YOLOv26, IoU is ignored since post-processing is handled internally by the model.
        return _yolo.RunObjectDetection(img, confidence, iou, roi);
    }

    /// <summary>
    /// 图像分割
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="pixelConfedence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <returns></returns>
    /// <remarks>
    /// 分割模型（如 yolov8n-seg.pt）
    /// </remarks>
    public IReadOnlyCollection<Segmentation> RunSegmentation(SKImage img, double confidence = 0.2, double pixelConfedence = 0.65, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunSegmentation(img, confidence, pixelConfedence, iou, roi);
    }

    /// <summary>
    /// 图像分割
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="pixelConfedence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <returns></returns>
    /// <remarks>
    /// 分割模型（如 yolov8n-seg.pt）
    /// </remarks>
    public IReadOnlyCollection<Segmentation> RunSegmentation(SKBitmap img, double confidence = 0.2, double pixelConfedence = 0.65, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunSegmentation(img, confidence, pixelConfedence, iou, roi);
    }

    /// <summary>
    /// 姿态估计 / 关键点检测
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <returns></returns>
    /// <remarks>
    /// YOLOv8 及后续版本提供了姿态估计模型（如 yolov8n-pose.pt）
    /// </remarks>
    public IReadOnlyCollection<PoseEstimation> RunPoseEstimation(SKImage img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunPoseEstimation(img, confidence, iou, roi);
    }

    /// <summary>
    /// 姿态估计 / 关键点检测
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <returns></returns>
    /// <remarks>
    /// YOLOv8 及后续版本提供了姿态估计模型（如 yolov8n-pose.pt）
    /// </remarks>
    public IReadOnlyCollection<PoseEstimation> RunPoseEstimation(SKBitmap img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunPoseEstimation(img, confidence, iou, roi);
    }

    /// <summary>
    /// 旋转目标检测。
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <remarks>
    /// YOLOv8 引入了 OBB 模型（如 yolov8n-obb.pt）
    /// </remarks>
    public IReadOnlyCollection<OBBDetection> RunObbDetection(SKImage img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunObbDetection(img, confidence, iou, roi);
    }

    /// <summary>
    /// 旋转目标检测。
    /// </summary>
    /// <param name="img"></param>
    /// <param name="confidence"></param>
    /// <param name="iou"></param>
    /// <param name="roi"></param>
    /// <remarks>
    /// YOLOv8 引入了 OBB 模型（如 yolov8n-obb.pt）
    /// </remarks>
    public IReadOnlyCollection<OBBDetection> RunObbDetection(SKBitmap img, double confidence = 0.2, double iou = 0.7, SKRectI? roi = null)
    {
        return _yolo.RunObbDetection(img, confidence, iou, roi);
    }

    public void Dispose()
    {
        _yolo.Dispose();
    }
}
