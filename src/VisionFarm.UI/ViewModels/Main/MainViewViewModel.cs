using Avalonia.Controls;
using Avalonia.Threading;
using VisionFarm.UI.Routes;

namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 主视图 ViewModel。
/// </summary>
public sealed partial class MainViewViewModel : ViewModelBase
{
    private readonly DispatcherTimer _updateCheckTimer; // 更新检查定时器

    public string? _activeMenuKey; // 已激活的菜单 Key

    /// <summary>
    /// 左侧应用标题。
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 左侧菜单。
    /// </summary>
    public MenuViewModel Menus { get; }

    /// <summary>
    /// 已激活的菜单项
    /// </summary>
    [ObservableProperty]
    public partial MenuItemViewModel? ActiveMenuItem { get; set; }

    /// <summary>
    /// Body 内容（ViewModel）
    /// </summary>
    [ObservableProperty]
    public partial object? Content { get; set; }

    /// <summary>
    /// 是否有新版本。
    /// </summary>
    [ObservableProperty]
    public partial bool HasNewVersion { get; set; }

    public MainViewViewModel()
    {
        // 注册弱引用消息，用于接收菜单 ViewModel 激活时发送的消息。
        WeakReferenceMessenger.Default.Register<MainViewViewModel, ValueChangedMessage<string>, string>(this, MessengerEmit.Navigation, async (r, m) =>
        {
            await OnNavigationAsync(m.Value);
        });

        // 从配置文件中获取信息
        var configuration = App.Current!.Services.GetRequiredService<IConfiguration>();
        Title = configuration["App:Title"] ?? "数字应用客户端";

        // 获取菜单
        Menus = App.Current!.Services.GetRequiredService<MenuViewModel>();

        // 启动后加载菜单配置首项
        var menuKey = Menus.MenuItems.FirstOrDefault()?.Key ?? "";
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(menuKey), MessengerEmit.Navigation);

        // 更新检查定时器
        _updateCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(2), // 间隔时间
        };
        _updateCheckTimer.Tick += UpdateCheckTimer_Tick;
        _updateCheckTimer.Start();
    }

    private async Task OnNavigationAsync(string menuKey)
    {
        if (string.IsNullOrEmpty(menuKey))
        {
            Content = new TextBlock { Text = "系统还没有配置菜单" };
            return;
        }

        // 当点击的菜单与已激活的菜单相同，直接返回
        if (_activeMenuKey == menuKey)
        {
            return;
        }

        // 释放上一导航页面的 ViewModel 资源
        if (Content is ViewModelBase viewModel && !viewModel.GetType().IsDefined(typeof(VmKeepAliveAttribute), false))
        {
            viewModel.Dispose();
        }

        // 激活菜单
        _activeMenuKey = menuKey;
        ActiveMenuItem = Menus.MenuItems.FirstOrDefault(s => s.Key == _activeMenuKey);

        var router = App.Current!.Services.GetRequiredService<IViewRouter>();
        var vmType = router.Routes.FirstOrDefault(s => menuKey.Equals(s.AliasName, StringComparison.OrdinalIgnoreCase))?.ViewModelType;
        if (vmType != null)
        {
            Content = App.Current?.Services.GetService(vmType);

            // ViewModel 初始化
            if (Content is ViewModelBase viewModel2)
            {
                viewModel2.SetHeader(ActiveMenuItem?.MenuHeader ?? string.Empty);
                viewModel2.SetAccessCodes(ActiveMenuItem?.AccessCodes ?? []);
                await viewModel2.OnInitializeAsync();
            }
        }
        else
        {
            Content = new TextBlock { Text = $"没找到指定的 {menuKey} 对应的 ViewModel" };
        }
    }

    private void UpdateCheckTimer_Tick(object? sender, EventArgs e)
    {
        // 定时器触发时进行检查
    }

    public override void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _updateCheckTimer.Stop();
        _updateCheckTimer.Tick -= UpdateCheckTimer_Tick;
    }
}
