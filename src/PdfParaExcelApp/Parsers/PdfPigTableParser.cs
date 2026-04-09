using System.Text;
using System.Text.RegularExpressions;
using PdfParaExcelApp.Helpers;
using PdfParaExcelApp.Models;
using PdfParaExcelApp.Services;
using UglyToad.PdfPig;

namespace PdfParaExcelApp.Parsers;

public class PdfPigTableParser(IColumnDetectorService columnDetector, IHeaderNormalizerService normalizer) : IPdfTableParser
{
    private static readonly string[] EssentialColumns = ["CODIGO", "DESCRICAO_ITEM", "UNIDADE", "QTD_SALDO"];

    private static readonly string[] StopTokens =
    [
        "PREFEITURA MUNICIPAL", "RELATORIO", "EXERCICIO", "MODALIDADE", "LICITACAO", "FORNECEDOR",
        "SITUACAO QUANTO AO SALDO", "TOTAL GERAL", "TOTAL:", "EMISSAO", "PAGINA"
    ];

    public ParsedTableModel Parse(string pdfPath, IProgress<string>? progress = null)
    {
        var debugFolder = Path.GetDirectoryName(pdfPath) ?? Environment.CurrentDirectory;
        var linesDebug = new StringBuilder();
        var headerDebug = new StringBuilder();
        var columnDebug = new StringBuilder();
        var rowsDebug = new StringBuilder();

        try
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
            LogLines(lines, linesDebug);

            var headerCandidates = FindHeaderCandidates(lines, headerDebug);
            var firstHeader = headerCandidates.FirstOrDefault()
                ?? throw new InvalidOperationException("Cabeçalho da tabela não encontrado. Verifique debug-header-detection.txt.");

            var columns = columnDetector.DetectColumns(firstHeader.HeaderLines, msg => columnDebug.AppendLine(msg)).ToList();
            var essentialDetected = EssentialColumns.Count(c => columns.Any(col => col.CanonicalName.Equals(c, StringComparison.OrdinalIgnoreCase)));
            if (essentialDetected < EssentialColumns.Length)
            {
                throw new InvalidOperationException("Colunas essenciais detectadas parcialmente. Verifique debug-column-map.txt.");
            }

            progress?.Report($"Tabela encontrada. {columns.Count} colunas detectadas. Extraindo itens...");

            var rows = ExtractRows(lines, columns, headerCandidates, rowsDebug);
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("Tabela encontrada, mas nenhum item foi reconhecido.");
            }

            return new ParsedTableModel
            {
                Columns = columns,
                Rows = rows
            };
        }
        finally
        {
            File.WriteAllText(Path.Combine(debugFolder, "debug-lines.txt"), linesDebug.ToString());
            File.WriteAllText(Path.Combine(debugFolder, "debug-header-detection.txt"), headerDebug.ToString());
            File.WriteAllText(Path.Combine(debugFolder, "debug-column-map.txt"), columnDebug.ToString());
            File.WriteAllText(Path.Combine(debugFolder, "debug-rows.txt"), rowsDebug.ToString());
        }
    }

    private List<PdfRowModel> ExtractRows(
        IReadOnlyList<RawPdfLine> lines,
        IReadOnlyList<ColumnDefinition> columns,
        IReadOnlyList<HeaderMatch> headerCandidates,
        StringBuilder rowsDebug)
    {
        var firstHeader = headerCandidates.First();
        var headerSet = BuildHeaderSkipSet(headerCandidates);
        var descriptionColumn = columns.First(c => normalizer.IsDescriptionColumn(c.CanonicalName));

        var rows = new List<PdfRowModel>();
        PdfRowModel? current = null;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.PageNumber < firstHeader.PageNumber)
            {
                continue;
            }

            if (line.PageNumber == firstHeader.PageNumber && line.Y > firstHeader.MinY)
            {
                continue;
            }

            if (headerSet.Contains((line.PageNumber, NormalizeY(line.Y))))
            {
                rowsDebug.AppendLine($"SKIP-HEADER P{line.PageNumber} Y{line.Y:F2}: {line.Text}");
                continue;
            }

            if (ShouldIgnoreLine(line.Text))
            {
                rowsDebug.AppendLine($"SKIP-NOISE P{line.PageNumber} Y{line.Y:F2}: {line.Text}");
                continue;
            }

            var row = MapLineToRow(line, columns);
            var code = row.GetValue("CODIGO") ?? string.Empty;
            var description = row.GetValue(descriptionColumn.CanonicalName) ?? string.Empty;

            if (LooksLikeCode(code))
            {
                rows.Add(row);
                current = row;
                rowsDebug.AppendLine($"NEW-ITEM P{line.PageNumber} Y{line.Y:F2} | CODIGO={code} | DESC={description}");
                continue;
            }

            if (current is not null && !string.IsNullOrWhiteSpace(description))
            {
                var old = current.GetValue(descriptionColumn.CanonicalName) ?? string.Empty;
                current.SetValue(descriptionColumn.CanonicalName, $"{old} {description}".Trim());
                rowsDebug.AppendLine($"CONT-DESC P{line.PageNumber} Y{line.Y:F2} | +{description}");
                continue;
            }

            rowsDebug.AppendLine($"SKIP-ROW P{line.PageNumber} Y{line.Y:F2} | {line.Text}");
        }

        return rows;
    }

    private static PdfRowModel MapLineToRow(RawPdfLine line, IReadOnlyList<ColumnDefinition> columns)
    {
        var row = new PdfRowModel();
        foreach (var column in columns)
        {
            row.SetValue(column.CanonicalName, string.Empty);
        }

        foreach (var word in line.Words.OrderBy(w => w.X))
        {
            var column = columns.FirstOrDefault(c => word.X >= c.XStart && word.X <= c.XEnd)
                ?? columns.OrderBy(c => Math.Abs(c.XStart - word.X)).First();

            var current = row.GetValue(column.CanonicalName);
            row.SetValue(column.CanonicalName,
                string.IsNullOrWhiteSpace(current) ? word.Text.Trim() : $"{current} {word.Text.Trim()}".Trim());
        }

        return row;
    }

    private static bool LooksLikeCode(string code)
    {
        var normalized = code.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        return Regex.IsMatch(normalized, "^[0-9]{1,6}([.-][0-9A-Z]+)?$", RegexOptions.CultureInvariant)
               || Regex.IsMatch(normalized, "^[0-9A-Z]{2,}$", RegexOptions.CultureInvariant);
    }

    private bool ShouldIgnoreLine(string lineText)
    {
        var normalized = normalizer.NormalizeText(lineText);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return true;
        }

        if (StopTokens.Any(token => normalized.Contains(token, StringComparison.Ordinal)))
        {
            return true;
        }

        if (Regex.IsMatch(normalized, "^\\d+\\s+\\d+$", RegexOptions.CultureInvariant)
            || Regex.IsMatch(normalized, "^\\d+\\s*/\\s*\\d+$", RegexOptions.CultureInvariant)
            || Regex.IsMatch(normalized, "^\\d{2}/\\d{2}/\\d{4}", RegexOptions.CultureInvariant))
        {
            return true;
        }

        return false;
    }

    private List<HeaderMatch> FindHeaderCandidates(IReadOnlyList<RawPdfLine> lines, StringBuilder headerDebug)
    {
        var candidates = new List<HeaderMatch>();

        var byPage = lines
            .GroupBy(l => l.PageNumber)
            .OrderBy(g => g.Key);

        foreach (var pageGroup in byPage)
        {
            var pageLines = pageGroup.OrderByDescending(l => l.Y).ToList();
            for (var i = 0; i < pageLines.Count; i++)
            {
                var window = pageLines.Skip(i).Take(3).ToList();
                if (window.Count == 0)
                {
                    continue;
                }

                var normalized = normalizer.NormalizeText(string.Join(" ", window.Select(l => l.Text)));
                var matchCount = ScoreHeader(normalized, out var tokens);
                headerDebug.AppendLine($"P{pageGroup.Key} idx={i} score={matchCount} tokens=[{string.Join(",", tokens)}] text='{normalized}'");

                if (matchCount >= 6)
                {
                    candidates.Add(new HeaderMatch(pageGroup.Key, window, window.Min(l => l.Y), window.Max(l => l.Y)));
                }
            }
        }

        headerDebug.AppendLine($"Total de candidatos de cabeçalho: {candidates.Count}");
        return candidates
            .OrderBy(c => c.PageNumber)
            .ThenByDescending(c => c.MaxY)
            .ToList();
    }

    private static int ScoreHeader(string normalizedText, out List<string> tokens)
    {
        tokens = [];
        var expected = new Dictionary<string, string[]>
        {
            ["CODIGO"] = ["CODIGO"],
            ["DESCRICAO"] = ["DESCRICAO"],
            ["UNID"] = ["UNID", "UNIDADE"],
            ["LICITADA"] = ["LICITADA"],
            ["ADITIVADA"] = ["ADITIVADA"],
            ["CONTRATADA"] = ["CONTRATADA"],
            ["COMPRA"] = ["COMPRA"],
            ["COMPRADA"] = ["COMPRADA"],
            ["EXPIRADA"] = ["EXPIRADA"],
            ["SALDO"] = ["SALDO"],
            ["VALOR_ITEM"] = ["VALOR ITEM"],
            ["VALOR_SALDO"] = ["VALOR SALDO"]
        };

        foreach (var item in expected)
        {
            if (item.Value.Any(v => normalizedText.Contains(v, StringComparison.Ordinal)))
            {
                tokens.Add(item.Key);
            }
        }

        return tokens.Count;
    }

    private static HashSet<(int Page, int Y)> BuildHeaderSkipSet(IReadOnlyList<HeaderMatch> matches)
    {
        var set = new HashSet<(int Page, int Y)>();
        foreach (var match in matches)
        {
            foreach (var line in match.HeaderLines)
            {
                set.Add((line.PageNumber, NormalizeY(line.Y)));
            }
        }

        return set;
    }

    private static int NormalizeY(double y)
        => (int)Math.Round(y * 10, MidpointRounding.AwayFromZero);

    private static void LogLines(IEnumerable<RawPdfLine> lines, StringBuilder output)
    {
        foreach (var page in lines.GroupBy(l => l.PageNumber).OrderBy(g => g.Key))
        {
            output.AppendLine($"=== PAGINA {page.Key} ===");
            foreach (var line in page.OrderByDescending(l => l.Y))
            {
                output.AppendLine($"Y={line.Y:F2} | {line.Text}");
            }

            output.AppendLine();
        }
    }

    private sealed record HeaderMatch(int PageNumber, IReadOnlyList<RawPdfLine> HeaderLines, double MinY, double MaxY);
}
