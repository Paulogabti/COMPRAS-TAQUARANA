using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Parsers;

public interface IPdfTableParser
{
    ParsedTableModel Parse(string pdfPath, IProgress<string>? progress = null);
}
