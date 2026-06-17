using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Irihi.Avalonia.Shared.Contracts;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace VisionIndustry.UI.Infrastructure;

/// <summary>
/// 弹窗对话页面的 ViewModel 基类。
/// </summary>
public abstract class DialogViewModelBase : ObservableObject, IViewModelObject, IViewToViewModelBridge, IDialogContext, IDisposable
{
    private bool _disposedValue;

    private WindowToastManager? _toastManager;
    private WindowNotificationManager? _notificationManager;

    private readonly WeakReference<Visual?> _visualRef = new(default);

    /// <summary>
    /// 请求窗口关闭事件
    /// </summary>
    public event EventHandler<object?>? RequestClose;

    Visual? IViewToViewModelBridge.View
    {
        get => _visualRef.TryGetTarget(out var target) ? target : default;
        set
        {
            _visualRef.SetTarget(value);
        }
    }

    /// <summary>
    /// 通过 ViewLocator，将 View 注入 ViewModel 中，以便能获取 TopLevel。
    /// </summary>
    public TopLevel? TopLevel
    {
        get
        {
            if (_visualRef.TryGetTarget(out var view) && view is not null)
            {
                return TopLevel.GetTopLevel(view);
            }
            return default;
        }
    }

    /// <summary>
    /// 可在此函数内进行数据初始化。
    /// </summary>
    /// <returns></returns>
    public virtual Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 显示提示消息。
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="notificationType">消息类型</param>
    protected void ShowToast(string message, NotificationType notificationType = NotificationType.Success)
    {
        if (_toastManager is null && _visualRef.TryGetTarget(out var view) && view is not null)
        {
            var visualLayerManager = view.FindAncestorOfType<VisualLayerManager>();
            _toastManager = WindowToastManager.TryGetToastManager(visualLayerManager, out var toastManager)
                ? toastManager
                : new WindowToastManager(visualLayerManager) { MaxItems = 3 };
        }

        _toastManager?.Show(
                new Toast(message),
                showIcon: false,
                showClose: false,
                type: notificationType,
                expiration: TimeSpan.FromSeconds(3),
                classes: ["Light"]);
    }

    /// <summary>
    /// 显示通知消息。
    /// </summary>
    /// <param name="title">消息标题</param>
    /// <param name="content">消息内容</param>
    /// <param name="notificationType">消息类型</param>
    protected void ShowNotification(
        string? title,
        string? content,
        NotificationType notificationType = NotificationType.Success)
    {
        if (_toastManager is null && _visualRef.TryGetTarget(out var view) && view is not null)
        {
            var visualLayerManager = view.FindAncestorOfType<VisualLayerManager>();
            _notificationManager = WindowNotificationManager.TryGetNotificationManager(visualLayerManager, out var notificationManager)
                ? notificationManager
                : new WindowNotificationManager(visualLayerManager) { MaxItems = 3 };
        }

        _notificationManager?.Show(
            new Notification(title, content),
            notificationType,
            expiration: TimeSpan.FromSeconds(5),
            showClose: false,
            classes: ["Light"]);
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void Close()
    {
        RequestClose?.Invoke(this, null);

        Dispose(true); // 关闭弹出对话框时触发
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _toastManager?.Uninstall();
                _notificationManager?.Uninstall();
                _visualRef.SetTarget(null);
            }

            _disposedValue = true;
        }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
