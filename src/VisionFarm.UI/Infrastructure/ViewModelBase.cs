using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace VisionFarm.UI.Infrastructure;

/// <summary>
/// ViewModel 基础类，实现该类的对象会被注入到服务中。
/// </summary>
public abstract class ViewModelBase : ObservableObject, IViewModelObject, IViewToViewModelBridge, IDisposable
{
    private bool _disposedValue;

    private WindowToastManager? _toastManager;
    private WindowNotificationManager? _notificationManager;

    private readonly WeakReference<Visual?> _visualRef = new(default);

    private readonly List<string> _accessCodes = [];

    Visual? IViewToViewModelBridge.View
    {
        get => _visualRef.TryGetTarget(out var target) ? target : default;
        set => _visualRef.SetTarget(value);
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
    /// Header
    /// </summary>
    public string? Header
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Header));
        }
    }

    /// <summary>
    /// 访问代码集合。
    /// </summary>
    public IReadOnlyCollection<string> AccessCodes => _accessCodes;

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
        _toastManager ??= new(TopLevel) { MaxItems = 3 };
        _toastManager.Show(
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
    /// <param name="postion">显示位置</param>
    protected void ShowNotification(
        string? title,
        string? content,
        NotificationType notificationType = NotificationType.Success,
        NotificationPosition postion = NotificationPosition.TopRight)
    {
        if (_notificationManager == null && TopLevel != null)
        {
            _notificationManager = new WindowNotificationManager(TopLevel)
            {
                Position = postion,
                MaxItems = 3,
            };
        }

        _notificationManager?.Show(
            new Notification(title, content),
            notificationType,
            expiration: TimeSpan.FromSeconds(5),
            showClose: false,
            classes: ["Light"]);
    }

    /// <summary>
    /// 复制文本到剪贴板。
    /// </summary>
    /// <param name="text">要复制的文本</param>
    /// <returns></returns>
    protected async Task CopyTextAsync(string? text)
    {
        if (text is not null)
        {
            var clipboard = TopLevel?.Clipboard;
            if (clipboard != null)
            {
                await ClipboardExtensions.SetTextAsync(clipboard, text);
            }
        }
    }

    /// <summary>
    /// 设置 Header。
    /// </summary>
    /// <param name="header">Header</param>
    public virtual void SetHeader(string header)
    {
        Header = header;
    }

    /// <summary>
    /// 设置访问代码。
    /// </summary>
    /// <param name="codes">访问代码集合。</param>
    public void SetAccessCodes(IEnumerable<string> codes)
    {
        _accessCodes.AddRange(codes);
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
