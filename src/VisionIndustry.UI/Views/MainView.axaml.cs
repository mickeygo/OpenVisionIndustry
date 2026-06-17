using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VisionIndustry.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 工具栏
    /// </summary>
    private void OpenToolsDialog(object? sender, RoutedEventArgs e)
    {
    }

    /// <summary>
    /// 设置
    /// </summary>
    private async void OpenSettingsDialog(object? sender, RoutedEventArgs e)
    {
        await DialogOverlay.Show<SettingsDialogPage, SettingsDialogViewModel>();
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    private async void OpenUpdaterDialog(object? sender, RoutedEventArgs e)
    {
    }
}
