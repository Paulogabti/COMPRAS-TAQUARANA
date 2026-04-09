namespace PdfParaExcelApp.Services;

public interface IHeaderNormalizerService
{
    string Normalize(string header);
    string ToDisplayName(string canonicalName);
    bool IsDescriptionColumn(string canonicalName);
}
