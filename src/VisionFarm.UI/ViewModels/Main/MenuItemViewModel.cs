namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 菜单项实体。
/// </summary>
public sealed class MenuItemViewModel : ObservableObject
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? MenuHeader { get; set; }

    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// 菜单名称关键字
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Badge 名称
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// 分割线
    /// </summary>
    public bool IsSeparator { get; set; }

    /// <summary>
    /// 可访问的标识集合。
    /// </summary>
    public IReadOnlyCollection<string> AccessCodes { get; init; } = [];

    /// <summary>
    /// 子菜单
    /// </summary>
    public ObservableCollection<MenuItemViewModel> Children { get; set; } = [];

    /// <summary>
    /// 菜单激活命令
    /// </summary>
    public ICommand ActivateCommand { get; set; }

    public MenuItemViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivate);
    }

    private void OnActivate()
    {
        if (IsSeparator || Key is null)
        {
            return;
        }

        // 激活后将菜单名称关键字发送给 MainViewViewModel。
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(Key), MessengerEmit.Navigation);
    }
}
