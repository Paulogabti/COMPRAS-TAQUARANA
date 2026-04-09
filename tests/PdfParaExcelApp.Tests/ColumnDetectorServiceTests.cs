using PdfParaExcelApp.Models;
using PdfParaExcelApp.Services;

namespace PdfParaExcelApp.Tests;

public class ColumnDetectorServiceTests
{
    [Fact]
    public void DetectColumns_Should_Return_Header_With_Canonical_Name()
    {
        var detector = new ColumnDetectorService(new HeaderNormalizerService());
        var line = new RawPdfLine
        {
            PageNumber = 1,
            Y = 500,
            Words =
            [
                new PdfWordModel("CÓDIGO", 10, 500, 30, 1),
                new PdfWordModel("DESCRIÇÃO", 90, 500, 60, 1),
                new PdfWordModel("DO", 152, 500, 15, 1),
                new PdfWordModel("ITEM", 169, 500, 25, 1),
                new PdfWordModel("UNID", 300, 500, 30, 1)
            ]
        };

        var result = detector.DetectColumns(line);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.CanonicalName == "DESCRICAO_ITEM");
    }
}
