namespace PdfParaExcelApp.Services;

public interface IFileDialogService
{
    string? PickPdfFile();
    string? PickOutputFile(string suggestedName);
}
