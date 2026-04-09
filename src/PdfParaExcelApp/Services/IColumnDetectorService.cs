using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public interface IColumnDetectorService
{
    IReadOnlyList<ColumnDefinition> DetectColumns(RawPdfLine headerLine);
}
