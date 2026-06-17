using VisionIndustry.UI.Routes;

namespace VisionIndustry.UI.ViewModels;

/// <summary>
/// 设置 ViewModel。
/// </summary>
public sealed partial class SettingsDialogViewModel : DialogViewModelBase
{
    private string? _activePageType;

    /// <summary>
    /// Body 内容（ViewModel）
    /// </summary>
    [ObservableProperty]
    public partial object? Content { get; set; }

    [RelayCommand]
    private async Task Activate(string pageType)
    {
        // 同一菜单项不重复触发
        if (_activePageType == pageType)
        {
            return;
        }

        _activePageType = pageType;

        // 释放当前菜单项的资源
        DisposeCurrentContent();

        var router = App.Current!.Services.GetRequiredService<IViewRouter>();
        var vmType = router.Routes.FirstOrDefault(s => s.AliasName == pageType)?.ViewModelType;
        if (vmType != null)
        {
            Content = App.Current?.Services.GetService(vmType);

            // ViewModel 初始化
            if (Content is ViewModelBase viewModel2)
            {
                await viewModel2.OnInitializeAsync();
            }
        }
        else
        {
            Content = null;
        }
    }

    public override void Close()
    {
        DisposeCurrentContent();

        base.Close();
    }

    private void DisposeCurrentContent()
    {
        // 释放当前菜单项的资源
        if (Content != null && Content is ViewModelBase viewModel && !viewModel.GetType().IsDefined(typeof(VmKeepAliveAttribute), false))
        {
            viewModel.Dispose();

            Content = null;
        }
    }
}
