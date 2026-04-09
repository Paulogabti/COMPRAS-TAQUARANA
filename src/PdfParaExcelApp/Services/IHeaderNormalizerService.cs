namespace PdfParaExcelApp.Services;

public interface IHeaderNormalizerService
{
    string Normalize(string header);
    string NormalizeText(string text);
    string ToDisplayName(string canonicalName);
    bool IsDescriptionColumn(string canonicalName);
}
