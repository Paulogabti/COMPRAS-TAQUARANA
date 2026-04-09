namespace PdfParaExcelApp.Models;

public class RawPdfLine
{
    public int PageNumber { get; init; }
    public double Y { get; init; }
    public List<PdfWordModel> Words { get; init; } = [];

    public string Text => string.Join(" ", Words.Select(w => w.Text));
}
