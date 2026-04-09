namespace PdfParaExcelApp.Models;

public class ParsedTableModel
{
    public List<ColumnDefinition> Columns { get; init; } = [];
    public List<PdfRowModel> Rows { get; init; } = [];
}
