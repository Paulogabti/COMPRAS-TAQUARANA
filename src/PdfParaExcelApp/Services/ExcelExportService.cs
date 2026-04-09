using ClosedXML.Excel;
using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public class ExcelExportService(IHeaderNormalizerService normalizer) : IExcelExportService
{
    public Task ExportAsync(ParsedTableModel table, IReadOnlyList<ColumnDefinition> selectedColumns, string outputPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Dados");

            for (var c = 0; c < selectedColumns.Count; c++)
            {
                var col = selectedColumns[c];
                ws.Cell(1, c + 1).Value = col.DisplayName;
            }

            for (var r = 0; r < table.Rows.Count; r++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (var c = 0; c < selectedColumns.Count; c++)
                {
                    var column = selectedColumns[c];
                    var value = table.Rows[r].GetValue(column.CanonicalName) ?? string.Empty;
                    ws.Cell(r + 2, c + 1).Value = value;
                }
            }

            var range = ws.Range(1, 1, Math.Max(1, table.Rows.Count + 1), Math.Max(1, selectedColumns.Count));
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            range.Style.Alignment.WrapText = true;

            var headerRange = ws.Range(1, 1, 1, Math.Max(1, selectedColumns.Count));
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
            ws.SheetView.FreezeRows(1);
            range.SetAutoFilter();

            ws.Columns().AdjustToContents(8, 60);

            for (var c = 0; c < selectedColumns.Count; c++)
            {
                var col = selectedColumns[c];
                if (normalizer.IsDescriptionColumn(col.CanonicalName))
                {
                    ws.Column(c + 1).Width = Math.Clamp(ws.Column(c + 1).Width + 20, 50, 90);
                }
            }

            workbook.SaveAs(outputPath);
        }, cancellationToken);
    }
}
