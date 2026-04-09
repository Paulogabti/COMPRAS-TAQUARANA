namespace PdfParaExcelApp.Models;

public sealed record PdfWordModel(string Text, double X, double Y, double Width, int PageNumber)
{
    public double XEnd => X + Width;
}
