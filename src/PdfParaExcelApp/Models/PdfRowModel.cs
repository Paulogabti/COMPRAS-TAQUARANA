namespace PdfParaExcelApp.Models;

public class PdfRowModel
{
    public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? GetValue(string columnName)
        => Values.TryGetValue(columnName, out var value) ? value : null;

    public void SetValue(string columnName, string value)
        => Values[columnName] = value;
}
