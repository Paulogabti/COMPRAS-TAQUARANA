using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Helpers;

public static class PdfLineGroupingHelper
{
    public static List<RawPdfLine> GroupWordsIntoLines(IEnumerable<PdfWordModel> words, double yTolerance = 2.5)
    {
        var sorted = words
            .OrderByDescending(w => w.Y)
            .ThenBy(w => w.X)
            .ToList();

        var lines = new List<RawPdfLine>();
        foreach (var word in sorted)
        {
            var line = lines.FirstOrDefault(l => l.PageNumber == word.PageNumber && Math.Abs(l.Y - word.Y) <= yTolerance);
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

        return lines
            .OrderBy(l => l.PageNumber)
            .ThenByDescending(l => l.Y)
            .ToList();
    }
}
