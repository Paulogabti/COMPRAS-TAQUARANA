using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public interface IColumnDetectorService
{
    IReadOnlyList<ColumnDefinition> DetectColumns(IReadOnlyList<RawPdfLine> headerLines, Action<string>? debugLog = null);
}
