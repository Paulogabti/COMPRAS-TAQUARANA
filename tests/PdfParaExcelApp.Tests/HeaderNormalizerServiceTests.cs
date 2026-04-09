using PdfParaExcelApp.Services;

namespace PdfParaExcelApp.Tests;

public class HeaderNormalizerServiceTests
{
    private readonly HeaderNormalizerService _service = new();

    [Theory]
    [InlineData("DESCRIÇÃO DO ITEM", "DESCRICAO_ITEM")]
    [InlineData("DESCRICAO ITEM", "DESCRICAO_ITEM")]
    [InlineData("QTD. SALDO", "QTD_SALDO")]
    [InlineData("QUANT. SALDO", "QTD_SALDO")]
    [InlineData("UNID", "UNIDADE")]
    public void Normalize_Should_Map_Equivalent_Headers(string input, string expected)
    {
        var result = _service.Normalize(input);
        Assert.Equal(expected, result);
    }
}
