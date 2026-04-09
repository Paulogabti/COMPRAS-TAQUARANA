namespace PdfParaExcelApp.Services;

public interface IUserSettingsService
{
    string? LastPdfPath { get; set; }
    string? LastOutputPath { get; set; }
    void Save();
}
