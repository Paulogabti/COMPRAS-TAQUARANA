using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public class ColumnDetectorService(IHeaderNormalizerService normalizer) : IColumnDetectorService
{
    public IReadOnlyList<ColumnDefinition> DetectColumns(RawPdfLine headerLine)
    {
        var sorted = headerLine.Words
            .OrderBy(w => w.X)
            .ToList();

        var columns = new List<ColumnDefinition>();

        foreach (var word in sorted)
        {
            if (string.IsNullOrWhiteSpace(word.Text))
            {
                continue;
            }

            if (columns.Count == 0)
            {
                columns.Add(NewColumn(word));
                continue;
            }

            var previous = columns[^1];
            if (word.X - previous.XEnd <= 8)
            {
                var merged = $"{previous.OriginalName} {word.Text}".Trim();
                columns[^1] = new ColumnDefinition
                {
                    OriginalName = merged,
                    CanonicalName = normalizer.Normalize(merged),
                    DisplayName = normalizer.ToDisplayName(normalizer.Normalize(merged)),
                    XStart = previous.XStart,
                    XEnd = Math.Max(previous.XEnd, word.XEnd),
                    IsSelected = true
                };

                continue;
            }

            columns.Add(NewColumn(word));
        }

        for (var i = 0; i < columns.Count; i++)
        {
            var nextStart = i + 1 < columns.Count ? columns[i + 1].XStart : columns[i].XEnd + 200;
            columns[i].XEnd = nextStart - 2;
        }

        return columns;
    }

    private ColumnDefinition NewColumn(PdfWordModel word)
    {
        var canonical = normalizer.Normalize(word.Text);
        return new ColumnDefinition
        {
            OriginalName = word.Text,
            CanonicalName = canonical,
            DisplayName = normalizer.ToDisplayName(canonical),
            XStart = word.X,
            XEnd = word.XEnd,
            IsSelected = true
        };
    }
}
