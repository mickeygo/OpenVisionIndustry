using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using VisionFarm.Common.Extensions;
using VisionFarm.UI.Extensions;
using VisionFarm.UI.Views;

namespace VisionFarm.UI;

public partial class App : Application
{
    private Mutex? _mutex;

    private readonly IHost _host = Host.CreateDefaultBuilder()
        .UseCommonSetup()
        .UseUISetup()
        .UseSerilog((hostingContext, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
        ).Build();

    /// <summary>
    /// 获取当前的 Application 应用程序实例。
    /// </summary>
    public static new App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 只允许开启一个应用（若是相同程序不同应用，需修改 name）
        _mutex = new(true, "VisionFarm.Desktop", out var createdNew);
        if (!createdNew)
        {
            Log.Information("应用程序已启动，不能同时开启多个。");
            Environment.Exit(0);
            return;
        }

        Log.Information("应用程序启动");

        _host.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewViewModel(),
            };
            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 中止应用。
    /// </summary>
    public async Task ShutdownAsync()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow?.DataContext is MainViewViewModel wm)
            {
                wm.Dispose();
            }
        }
    }
}
