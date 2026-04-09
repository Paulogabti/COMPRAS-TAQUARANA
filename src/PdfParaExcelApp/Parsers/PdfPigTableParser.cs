using System.Text.RegularExpressions;
using PdfParaExcelApp.Helpers;
using PdfParaExcelApp.Models;
using PdfParaExcelApp.Services;
using UglyToad.PdfPig;

namespace PdfParaExcelApp.Parsers;

public class PdfPigTableParser(IColumnDetectorService columnDetector, IHeaderNormalizerService normalizer) : IPdfTableParser
{
    private static readonly string[] HeaderKeywords = ["CODIGO", "DESCRICAO", "UNID", "QTD", "VALOR"];
    private static readonly string[] StopTokens = ["TOTAL GERAL", "TOTAL:", "EMISSAO", "PAGINA", "RELATORIO", "PREFEITURA"];

    public ParsedTableModel Parse(string pdfPath, IProgress<string>? progress = null)
    {
        using var doc = PdfDocument.Open(pdfPath);

        progress?.Report("Lendo páginas do PDF...");
        var words = new List<PdfWordModel>();
        for (var pageIndex = 1; pageIndex <= doc.NumberOfPages; pageIndex++)
        {
            var page = doc.GetPage(pageIndex);
            foreach (var w in page.GetWords())
            {
                words.Add(new PdfWordModel(w.Text, w.BoundingBox.Left, w.BoundingBox.Bottom, w.BoundingBox.Width, pageIndex));
            }
        }

        var lines = PdfLineGroupingHelper.GroupWordsIntoLines(words);

        var firstHeaderLine = lines.FirstOrDefault(IsHeaderLine)
            ?? throw new InvalidOperationException("Nenhuma tabela compatível foi encontrada no PDF.");

        var columns = columnDetector.DetectColumns(firstHeaderLine).ToList();
        if (columns.Count == 0)
        {
            throw new InvalidOperationException("Não foi possível identificar colunas no cabeçalho.");
        }

        progress?.Report($"{columns.Count} colunas detectadas. Extraindo linhas...");

        var extractedRows = new List<PdfRowModel>();
        PdfRowModel? current = null;

        foreach (var line in lines)
        {
            if (line.PageNumber < firstHeaderLine.PageNumber)
            {
                continue;
            }

            if (IsHeaderLine(line))
            {
                continue;
            }

            var upper = NormalizeForSearch(line.Text);
            if (StopTokens.Any(token => upper.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (IsPageCounter(line.Text))
            {
                continue;
            }

            var row = MapLineToRow(line, columns);
            var codeValue = row.GetValue("CODIGO") ?? string.Empty;
            var hasCode = Regex.IsMatch(codeValue, "^[0-9A-Za-z.-]+$");

            var descriptionCanonical = columns.FirstOrDefault(c => normalizer.IsDescriptionColumn(c.CanonicalName))?.CanonicalName;
            var description = descriptionCanonical is null ? string.Empty : (row.GetValue(descriptionCanonical) ?? string.Empty);

            if (hasCode)
            {
                current = row;
                extractedRows.Add(row);
            }
            else if (current is not null && !string.IsNullOrWhiteSpace(description))
            {
                MergeContinuationRow(current, row, columns);
            }
            else
            {
                // Fallback textual: quando posições falham, tenta identificar pelo texto total e continuar descrição.
                if (current is not null && line.Text.Length > 8 && !LooksLikeNoise(line.Text))
                {
                    AppendTextFallback(current, line.Text, columns);
                }
            }
        }

        return new ParsedTableModel
        {
            Columns = columns,
            Rows = extractedRows
        };
    }

    private static PdfRowModel MapLineToRow(RawPdfLine line, IReadOnlyList<ColumnDefinition> columns)
    {
        var row = new PdfRowModel();
        foreach (var column in columns)
        {
            row.SetValue(column.CanonicalName, string.Empty);
        }

        foreach (var word in line.Words)
        {
            var column = columns
                .FirstOrDefault(c => word.X >= c.XStart - 2 && word.X <= c.XEnd + 2)
                ?? columns.OrderBy(c => Math.Abs(c.XStart - word.X)).First();

            var existing = row.GetValue(column.CanonicalName);
            var combined = string.IsNullOrWhiteSpace(existing)
                ? word.Text
                : $"{existing} {word.Text}";

            row.SetValue(column.CanonicalName, combined.Trim());
        }

        return row;
    }

    private void MergeContinuationRow(PdfRowModel current, PdfRowModel continuation, IReadOnlyList<ColumnDefinition> columns)
    {
        foreach (var col in columns)
        {
            var newValue = continuation.GetValue(col.CanonicalName);
            if (string.IsNullOrWhiteSpace(newValue))
            {
                continue;
            }

            var oldValue = current.GetValue(col.CanonicalName);
            if (string.IsNullOrWhiteSpace(oldValue))
            {
                current.SetValue(col.CanonicalName, newValue.Trim());
                continue;
            }

            current.SetValue(col.CanonicalName, $"{oldValue} {newValue}".Trim());
        }
    }

    private void AppendTextFallback(PdfRowModel current, string rawText, IReadOnlyList<ColumnDefinition> columns)
    {
        var descriptionColumn = columns.FirstOrDefault(c => normalizer.IsDescriptionColumn(c.CanonicalName));
        if (descriptionColumn is null)
        {
            return;
        }

        var old = current.GetValue(descriptionColumn.CanonicalName) ?? string.Empty;
        current.SetValue(descriptionColumn.CanonicalName, $"{old} {rawText}".Trim());
    }

    private static bool IsHeaderLine(RawPdfLine line)
    {
        var text = NormalizeForSearch(line.Text);
        return HeaderKeywords.Count(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase)) >= 3;
    }

    private static bool IsPageCounter(string text)
        => Regex.IsMatch(text.Trim(), "^\\d+\\s*/\\s*\\d+$");

    private static bool LooksLikeNoise(string text)
    {
        var normalized = NormalizeForSearch(text);
        return StopTokens.Any(token => normalized.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeForSearch(string text)
        => text.ToUpperInvariant()
            .Replace("Ç", "C")
            .Replace("Ã", "A")
            .Replace("Á", "A")
            .Replace("É", "E");
}
