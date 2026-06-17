using Avalonia.Controls;
using Avalonia.Controls.Templates;
using VisionIndustry.UI.Routes;

namespace VisionIndustry.UI.DataTemplates;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        // 若 ViewModel 为单例且有设置 View，直接复用 View。
        if (data.GetType().IsDefined(typeof(VmKeepAliveAttribute), false)
            && data is IViewToViewModelBridge bridge0
            && bridge0.View != null)
        {
            return (Control)bridge0.View;
        }

        var router = App.Current!.Services.GetRequiredService<IViewRouter>();
        var pageType = router.Routes.FirstOrDefault(s => s.ViewModelType == data.GetType())?.ViewType;
        if (pageType != null)
        {
            try
            {
                var control = (Control)Activator.CreateInstance(pageType)!;
                control.DataContext = data;

                // 桥接器，将 View 注入 ViewModel 中，以便能获取 TopLevel
                if (data is IViewToViewModelBridge bridge)
                {
                    bridge.View = control;
                }

                return control;
            }
            catch (Exception ex)
            {
                return new TextBlock { Text = $"Exception: {ex.Message} " };
            }
        }

        return new TextBlock { Text = $"Not Found: {data.GetType().Name} " };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
