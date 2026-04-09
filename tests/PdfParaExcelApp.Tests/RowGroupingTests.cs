using PdfParaExcelApp.Helpers;
using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Tests;

public class RowGroupingTests
{
    [Fact]
    public void GroupWordsIntoLines_Should_Group_Close_Y_Words_In_Same_Line()
    {
        var words = new List<PdfWordModel>
        {
            new("001", 10, 100, 12, 1),
            new("ITEM", 30, 100.8, 25, 1),
            new("002", 10, 80, 12, 1)
        };

        var lines = PdfLineGroupingHelper.GroupWordsIntoLines(words, 2.0);

        Assert.Equal(2, lines.Count);
        Assert.Equal("001 ITEM", lines[0].Text);
    }
}
