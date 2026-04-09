using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfParaExcelApp.Models;
using PdfParaExcelApp.Services;

namespace PdfParaExcelApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IPdfTableParserService _parserService;
    private readonly IExcelExportService _excelExportService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IUserSettingsService _settings;

    private ParsedTableModel? _parsedTable;

    public MainViewModel(
        IPdfTableParserService parserService,
        IExcelExportService excelExportService,
        IFileDialogService fileDialogService,
        IUserSettingsService settings)
    {
        _parserService = parserService;
        _excelExportService = excelExportService;
        _fileDialogService = fileDialogService;
        _settings = settings;

        PdfPath = _settings.LastPdfPath ?? string.Empty;
        OutputPath = _settings.LastOutputPath ?? string.Empty;
    }

    [ObservableProperty]
    private string _pdfPath = string.Empty;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Pronto para analisar o PDF.";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private string _previewSummary = "Nenhum arquivo carregado.";

    public ObservableCollection<ColumnDefinition> DetectedColumns { get; } = [];
    public ObservableCollection<string> PreviewRows { get; } = [];

    [RelayCommand]
    private void SelectPdf()
    {
        var selected = _fileDialogService.PickPdfFile();
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        PdfPath = selected;
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            var dir = Path.GetDirectoryName(selected) ?? Environment.CurrentDirectory;
            OutputPath = Path.Combine(dir, $"{Path.GetFileNameWithoutExtension(selected)}.xlsx");
        }
    }

    [RelayCommand]
    private void SelectOutput()
    {
        var suggested = string.IsNullOrWhiteSpace(OutputPath) ? "exportacao.xlsx" : Path.GetFileName(OutputPath);
        var selected = _fileDialogService.PickOutputFile(suggested);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            OutputPath = selected;
        }
    }

    [RelayCommand]
    private async Task AnalyzePdfAsync()
    {
        if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath))
        {
            StatusMessage = "Selecione um arquivo PDF válido.";
            return;
        }

        try
        {
            IsBusy = true;
            ProgressValue = 15;
            StatusMessage = "Iniciando análise do PDF...";

            var progress = new Progress<string>(msg => StatusMessage = msg);
            _parsedTable = await _parserService.ParseAsync(PdfPath, progress);

            DetectedColumns.Clear();
            foreach (var col in _parsedTable.Columns)
            {
                DetectedColumns.Add(col);
            }

            PreviewRows.Clear();
            foreach (var row in _parsedTable.Rows.Take(8))
            {
                var preview = string.Join(" | ", _parsedTable.Columns.Take(4).Select(c => $"{c.DisplayName}: {row.GetValue(c.CanonicalName)}"));
                PreviewRows.Add(preview);
            }

            PreviewSummary = $"{_parsedTable.Rows.Count} linhas detectadas. Prévia de até 8 linhas exibida.";
            ProgressValue = 100;
            StatusMessage = "PDF analisado com sucesso.";
            _settings.LastPdfPath = PdfPath;
            _settings.Save();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Falha ao ler PDF: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAllColumns()
    {
        foreach (var col in DetectedColumns)
        {
            col.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAllColumns()
    {
        foreach (var col in DetectedColumns)
        {
            col.IsSelected = false;
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (_parsedTable is null)
        {
            StatusMessage = "Analise um PDF antes de exportar.";
            return;
        }

        var selectedColumns = DetectedColumns.Where(c => c.IsSelected).ToList();
        if (selectedColumns.Count == 0)
        {
            StatusMessage = "Selecione ao menos uma coluna para exportar.";
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            StatusMessage = "Defina o caminho de saída do Excel.";
            return;
        }

        if (!OutputPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "O arquivo de saída deve ter extensão .xlsx.";
            return;
        }

        try
        {
            IsBusy = true;
            ProgressValue = 30;
            StatusMessage = "Gerando planilha Excel...";
            await _excelExportService.ExportAsync(_parsedTable, selectedColumns, OutputPath);
            ProgressValue = 100;
            StatusMessage = "Excel gerado com sucesso.";
            _settings.LastOutputPath = OutputPath;
            _settings.Save();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Falha ao gerar Excel: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task HandlePdfDropAsync(string path)
    {
        PdfPath = path;
        await AnalyzePdfAsync();
    }
}
