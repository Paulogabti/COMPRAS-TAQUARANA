using ClosedXML.Excel;
using PdfParaExcelApp.Models;
using PdfParaExcelApp.Services;

namespace PdfParaExcelApp.Tests;

public class ExcelExportServiceTests
{
    [Fact]
    public async Task ExportAsync_Should_Export_Only_Selected_Columns()
    {
        var normalizer = new HeaderNormalizerService();
        var service = new ExcelExportService(normalizer);

        var table = new ParsedTableModel
        {
            Columns =
            [
                new ColumnDefinition { OriginalName = "CODIGO", CanonicalName = "CODIGO", DisplayName = "CÓDIGO", XStart = 0, XEnd = 10 },
                new ColumnDefinition { OriginalName = "DESCRICAO", CanonicalName = "DESCRICAO_ITEM", DisplayName = "DESCRIÇÃO DO ITEM", XStart = 10, XEnd = 200 },
                new ColumnDefinition { OriginalName = "UNID", CanonicalName = "UNIDADE", DisplayName = "UNID", XStart = 200, XEnd = 250 }
            ],
            Rows =
            [
                new PdfRowModel
                {
                    Values =
                    {
                        ["CODIGO"] = "1",
                        ["DESCRICAO_ITEM"] = "TESOURA DE PODA",
                        ["UNIDADE"] = "UN"
                    }
                }
            ]
        };

        var selected = table.Columns.Where(c => c.CanonicalName != "CODIGO").ToList();
        var path = Path.Combine(Path.GetTempPath(), $"teste-export-{Guid.NewGuid():N}.xlsx");

        await service.ExportAsync(table, selected, path);

        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheet(1);
        Assert.Equal("DESCRIÇÃO DO ITEM", ws.Cell(1, 1).GetString());
        Assert.Equal("UNID", ws.Cell(1, 2).GetString());
        Assert.Equal("TESOURA DE PODA", ws.Cell(2, 1).GetString());

        File.Delete(path);
    }
}
