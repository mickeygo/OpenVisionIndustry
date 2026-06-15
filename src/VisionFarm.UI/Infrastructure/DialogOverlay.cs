using Avalonia.Controls;

namespace VisionFarm.UI.Infrastructure;

/// <summary>
/// OverlayDialog 封装对象。
/// </summary>
internal static class DialogOverlay
{
    /// <summary>
    /// 显示 Dialog 对话框。
    /// </summary>
    /// <typeparam name="TView">Control</typeparam>
    /// <typeparam name="TViewModel">ViewModel</typeparam>
    /// <param name="action">Dialog 选项设定</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Show<TView, TViewModel>(Action<OverlayDialogOptions>? action = null, CancellationToken cancellationToken = default)
        where TView : Control, new()
    {
        var options = new OverlayDialogOptions()
        {
            Mode = DialogMode.None,
            Buttons = DialogButton.None,
            IsCloseButtonVisible = true,
            CanDragMove = true,
            CanResize = false,
        };
        action?.Invoke(options);

        var control = new TView();
        var vm = App.Current!.Services.GetRequiredService(typeof(TViewModel));
        if (vm is IViewToViewModelBridge bridge)
        {
            bridge.View = control;
        }
        OverlayDialog.ShowCustom(control, vm, options: options);

        // ViewModel 初始化
        if (vm is DialogViewModelBase vmBase)
        {
            await vmBase.OnInitializeAsync(cancellationToken);
        }
    }
}
