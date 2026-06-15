using Avalonia.Controls.Notifications;

namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 菜单设置。
/// </summary>
public sealed partial class MenuSettingsViewModel(IAppData appData) : ViewModelBase
{
    /// <summary>
    /// 菜单选项
    /// </summary>
    public ObservableCollection<MenuItemSelectedViewModel> MenuItems { get; } = [];

    public override Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        var keys = appData.GetMenus()?.Keys ?? [];
        foreach (var menuItem in AppMenus.Menus)
        {
            MenuItems.Add(new MenuItemSelectedViewModel
            {
                MenuHeader = menuItem.MenuHeader,
                IconName = menuItem.IconName,
                Key = menuItem.Key,
                IsSelected = keys.Contains(menuItem.Key),
                AccessCodes = menuItem.AccessCodes,
            });
        }

        return base.OnInitializeAsync(cancellationToken);
    }

    [RelayCommand]
    private void Submit()
    {
        var keys = MenuItems.Where(s => s.IsSelected).Select(s => s.Key).ToArray();
        appData.SetMenus(new MenuProfile
        {
            Keys = keys,
        });

        ShowNotification("作业提示", "设置成功，重启应用程序后生效", NotificationType.Success);
    }
}
