using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfParaExcelApp.Services;

public class HeaderNormalizerService : IHeaderNormalizerService
{
    private static readonly Dictionary<string, string> Equivalences = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CODIGO"] = "CODIGO",
        ["DESCRICAO DO ITEM"] = "DESCRICAO_ITEM",
        ["DESCRICAO ITEM"] = "DESCRICAO_ITEM",
        ["DESCRICAO"] = "DESCRICAO_ITEM",
        ["UNID"] = "UNIDADE",
        ["UNIDADE"] = "UNIDADE",
        ["QTD SALDO"] = "QTD_SALDO",
        ["QTD. SALDO"] = "QTD_SALDO",
        ["QUANT SALDO"] = "QTD_SALDO",
        ["QUANT. SALDO"] = "QTD_SALDO",
        ["QTD COMPRADA"] = "QTD_COMPRADA",
        ["QTD LICITADA"] = "QTD_LICITADA",
        ["VALOR ITEM R$"] = "VALOR_ITEM",
        ["VALOR SALDO R$"] = "VALOR_SALDO"
    };

    public string Normalize(string header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        var cleaned = RemoveAccents(header).ToUpperInvariant();
        cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();
        cleaned = cleaned.Replace("º", string.Empty).Replace("°", string.Empty);

        if (Equivalences.TryGetValue(cleaned, out var canonical))
        {
            return canonical;
        }

        cleaned = cleaned.Replace(".", string.Empty).Replace(":", string.Empty).Trim();

        if (Equivalences.TryGetValue(cleaned, out canonical))
        {
            return canonical;
        }

        return cleaned.Replace(" ", "_");
    }

    public string ToDisplayName(string canonicalName)
        => canonicalName switch
        {
            "CODIGO" => "CÓDIGO",
            "DESCRICAO_ITEM" => "DESCRIÇÃO DO ITEM",
            "UNIDADE" => "UNID",
            "QTD_SALDO" => "QTD. SALDO",
            "QTD_COMPRADA" => "QTD. COMPRADA",
            "QTD_LICITADA" => "QTD. LICITADA",
            "VALOR_ITEM" => "VALOR ITEM (R$)",
            "VALOR_SALDO" => "VALOR SALDO (R$)",
            _ => canonicalName.Replace("_", " ")
        };

    public bool IsDescriptionColumn(string canonicalName)
        => canonicalName.Equals("DESCRICAO_ITEM", StringComparison.OrdinalIgnoreCase);

    private static string RemoveAccents(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
