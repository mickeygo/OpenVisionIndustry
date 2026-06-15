namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 菜单 ViewModel。
/// </summary>
public sealed class MenuViewModel : ViewModelBase
{
    /// <summary>
    /// 菜单项集合
    /// </summary>
    public ObservableCollection<MenuItemViewModel> MenuItems { get; } = [];

    public MenuViewModel(IAppData appData)
    {
        var menuKeys = appData.GetMenus()?.Keys ?? [];

        foreach (var menuItem in AppMenus.Menus.Where(s => menuKeys.Contains(s.Key)))
        {
            MenuItemViewModel menuItemViewModel = new()
            {
                MenuHeader = menuItem.MenuHeader,
                IconName = menuItem.IconName,
                Key = menuItem.Key,
                AccessCodes = menuItem.AccessCodes,
                //Children = new ObservableCollection(menuItem.Children),
            };
            MenuItems.Add(menuItemViewModel);
        }
    }
}
