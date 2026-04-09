using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Helpers;

public static class PdfLineGroupingHelper
{
    public static List<RawPdfLine> GroupWordsIntoLines(
        IEnumerable<PdfWordModel> words,
        double yTolerance = 2.8,
        bool mergeNearbyLines = true,
        double mergeTolerance = 1.2)
    {
        var sorted = words
            .OrderBy(w => w.PageNumber)
            .ThenByDescending(w => w.Y)
            .ThenBy(w => w.X)
            .ToList();

        var lines = new List<RawPdfLine>();
        foreach (var word in sorted)
        {
            var line = lines.FirstOrDefault(l =>
                l.PageNumber == word.PageNumber
                && Math.Abs(l.Y - word.Y) <= yTolerance);

            if (line is null)
            {
                line = new RawPdfLine
                {
                    PageNumber = word.PageNumber,
                    Y = word.Y,
                    Words = []
                };
                lines.Add(line);
            }

            line.Words.Add(word);
        }

        foreach (var line in lines)
        {
            line.Words.Sort((a, b) => a.X.CompareTo(b.X));
        }

        var ordered = lines
            .OrderBy(l => l.PageNumber)
            .ThenByDescending(l => l.Y)
            .ToList();

        if (!mergeNearbyLines)
        {
            return ordered;
        }

        var merged = new List<RawPdfLine>();
        foreach (var line in ordered)
        {
            var previous = merged.LastOrDefault();
            if (previous is null || previous.PageNumber != line.PageNumber)
            {
                merged.Add(CloneLine(line));
                continue;
            }

            var yDistance = Math.Abs(previous.Y - line.Y);
            var shouldMerge = yDistance <= mergeTolerance && HasHorizontalOverlap(previous, line);

            if (!shouldMerge)
            {
                merged.Add(CloneLine(line));
                continue;
            }

            previous.Words.AddRange(line.Words);
            previous.Words.Sort((a, b) => a.X.CompareTo(b.X));
        }

        return merged;
    }

    private static RawPdfLine CloneLine(RawPdfLine source)
        => new()
        {
            PageNumber = source.PageNumber,
            Y = source.Y,
            Words = [.. source.Words]
        };

    private static bool HasHorizontalOverlap(RawPdfLine first, RawPdfLine second)
    {
        if (first.Words.Count == 0 || second.Words.Count == 0)
        {
            return false;
        }

        var firstStart = first.Words.Min(w => w.X);
        var firstEnd = first.Words.Max(w => w.XEnd);
        var secondStart = second.Words.Min(w => w.X);
        var secondEnd = second.Words.Max(w => w.XEnd);

        return !(secondStart > firstEnd + 20 || firstStart > secondEnd + 20);
    }
}
