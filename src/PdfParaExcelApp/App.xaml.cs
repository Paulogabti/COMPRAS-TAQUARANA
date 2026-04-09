using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PdfParaExcelApp.Helpers;
using PdfParaExcelApp.Parsers;
using PdfParaExcelApp.Services;
using PdfParaExcelApp.ViewModels;
using PdfParaExcelApp.Views;

namespace PdfParaExcelApp;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var window = _serviceProvider.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHeaderNormalizerService, HeaderNormalizerService>();
        services.AddSingleton<IColumnDetectorService, ColumnDetectorService>();
        services.AddSingleton<IPdfTableParser, PdfPigTableParser>();
        services.AddSingleton<IPdfTableParserService, PdfTableParserService>();
        services.AddSingleton<IExcelExportService, ExcelExportService>();
        services.AddSingleton<IUserSettingsService, UserSettingsService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
