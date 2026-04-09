using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public class ColumnDetectorService(IHeaderNormalizerService normalizer) : IColumnDetectorService
{
    private sealed record ColumnSpec(string CanonicalName, params string[] AnchorTokens);

    private static readonly ColumnSpec[] OrderedSpecs =
    [
        new("CODIGO", "CODIGO"),
        new("DESCRICAO_ITEM", "DESCRICAO"),
        new("UNIDADE", "UNID", "UNIDADE"),
        new("QTD_LICITADA", "LICITADA"),
        new("QTD_ADITIVADA", "ADITIVADA"),
        new("QTD_CONTRATADA", "CONTRATADA"),
        new("QTD_EM_COMPRA", "COMPRA"),
        new("QTD_COMPRADA", "COMPRADA"),
        new("QTD_EXPIRADA", "EXPIRADA"),
        new("QTD_SALDO", "SALDO"),
        new("VALOR_ITEM", "ITEM"),
        new("VALOR_SALDO", "SALDO")
    ];

    public IReadOnlyList<ColumnDefinition> DetectColumns(IReadOnlyList<RawPdfLine> headerLines, Action<string>? debugLog = null)
    {
        var words = headerLines
            .SelectMany(l => l.Words)
            .OrderBy(w => w.X)
            .ToList();

        var columns = new List<ColumnDefinition>();
        if (words.Count == 0)
        {
            debugLog?.Invoke("Nenhuma palavra disponível nas linhas de cabeçalho.");
            return columns;
        }

        var startByCanonical = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var spec in OrderedSpecs)
        {
            var start = FindColumnStart(spec.CanonicalName, spec.AnchorTokens, words, debugLog);
            if (start.HasValue)
            {
                startByCanonical[spec.CanonicalName] = start.Value;
            }
        }

        foreach (var spec in OrderedSpecs)
        {
            if (!startByCanonical.TryGetValue(spec.CanonicalName, out var start))
            {
                debugLog?.Invoke($"Coluna sem âncora detectada: {spec.CanonicalName}");
                continue;
            }

            columns.Add(new ColumnDefinition
            {
                CanonicalName = spec.CanonicalName,
                OriginalName = normalizer.ToDisplayName(spec.CanonicalName),
                DisplayName = normalizer.ToDisplayName(spec.CanonicalName),
                XStart = start,
                XEnd = start,
                IsSelected = true
            });
        }

        columns = columns.OrderBy(c => c.XStart).ToList();
        for (var i = 0; i < columns.Count; i++)
        {
            var end = i + 1 < columns.Count
                ? ((columns[i].XStart + columns[i + 1].XStart) / 2d) - 1d
                : words.Max(w => w.XEnd) + 12d;

            columns[i].XEnd = Math.Max(columns[i].XStart + 1d, end);
            debugLog?.Invoke($"{columns[i].CanonicalName}: XStart={columns[i].XStart:F2} XEnd={columns[i].XEnd:F2}");
        }

        return columns;
    }

    private double? FindColumnStart(
        string canonicalName,
        IReadOnlyList<string> anchorTokens,
        IReadOnlyList<PdfWordModel> words,
        Action<string>? debugLog)
    {
        var candidates = words
            .Where(w => anchorTokens.Any(token => normalizer.NormalizeText(w.Text).Contains(token, StringComparison.Ordinal)))
            .OrderBy(w => w.X)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        PdfWordModel? selected = canonicalName switch
        {
            "QTD_EM_COMPRA" => candidates.FirstOrDefault(c =>
                normalizer.NormalizeText(c.Text).Contains("COMPRA", StringComparison.Ordinal)
                && words.Any(left => left.X < c.X
                    && c.X - left.X < 40
                    && normalizer.NormalizeText(left.Text) == "EM")),
            "QTD_SALDO" => candidates.FirstOrDefault(c =>
                normalizer.NormalizeText(c.Text).Contains("SALDO", StringComparison.Ordinal)
                && !words.Any(left => left.X < c.X
                    && c.X - left.X < 50
                    && normalizer.NormalizeText(left.Text).Contains("VALOR", StringComparison.Ordinal))),
            "VALOR_ITEM" => candidates.FirstOrDefault(c =>
                normalizer.NormalizeText(c.Text).Contains("ITEM", StringComparison.Ordinal)
                && words.Any(left => left.X < c.X
                    && c.X - left.X < 50
                    && normalizer.NormalizeText(left.Text).Contains("VALOR", StringComparison.Ordinal))),
            "VALOR_SALDO" => candidates.LastOrDefault(c =>
                normalizer.NormalizeText(c.Text).Contains("SALDO", StringComparison.Ordinal)
                && words.Any(left => left.X < c.X
                    && c.X - left.X < 50
                    && normalizer.NormalizeText(left.Text).Contains("VALOR", StringComparison.Ordinal))),
            _ => candidates.FirstOrDefault()
        };

        selected ??= candidates.FirstOrDefault();
        if (selected is null)
        {
            return null;
        }

        debugLog?.Invoke($"Âncora {canonicalName}: '{selected.Text}' X={selected.X:F2}");
        return selected.X;
    }
}
