using Avalonia.Controls;

namespace VisionFarm.UI.Views;

public partial class StatusBarBottomContent : UserControl
{
    public StatusBarBottomContent()
    {
        InitializeComponent();

        var vm = App.Current!.Services.GetRequiredService<StatusBarViewModel>();
        DataContext = vm;
    }
}
