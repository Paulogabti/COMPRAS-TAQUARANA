using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfParaExcelApp.Services;

public class HeaderNormalizerService : IHeaderNormalizerService
{
    private static readonly Dictionary<string, string> Equivalences = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CODIGO"] = "CODIGO",

        ["DESCRICAO"] = "DESCRICAO_ITEM",
        ["DESCRICAO ITEM"] = "DESCRICAO_ITEM",
        ["DESCRICAO DO ITEM"] = "DESCRICAO_ITEM",

        ["UNID"] = "UNIDADE",
        ["UNIDADE"] = "UNIDADE",

        ["QTD LICITADA"] = "QTD_LICITADA",
        ["QTD ADITIVADA"] = "QTD_ADITIVADA",
        ["QTD CONTRATADA"] = "QTD_CONTRATADA",
        ["QTD EM COMPRA"] = "QTD_EM_COMPRA",
        ["QTD COMPRADA"] = "QTD_COMPRADA",
        ["QTD EXPIRADA"] = "QTD_EXPIRADA",
        ["QTD SALDO"] = "QTD_SALDO",

        ["VALOR ITEM"] = "VALOR_ITEM",
        ["VALOR ITEM R$"] = "VALOR_ITEM",
        ["VALOR SALDO"] = "VALOR_SALDO",
        ["VALOR SALDO R$"] = "VALOR_SALDO"
    };

    public string Normalize(string header)
    {
        var normalized = NormalizeText(header)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (Equivalences.TryGetValue(normalized, out var canonical))
        {
            return canonical;
        }

        return normalized.Replace(" ", "_", StringComparison.Ordinal);
    }

    public string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var cleaned = RemoveAccents(text).ToUpperInvariant();
        cleaned = cleaned.Replace("º", string.Empty, StringComparison.Ordinal)
            .Replace("°", string.Empty, StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("/", " ", StringComparison.Ordinal)
            .Replace("(", " ", StringComparison.Ordinal)
            .Replace(")", " ", StringComparison.Ordinal)
            .Replace(";", " ", StringComparison.Ordinal);

        cleaned = Regex.Replace(cleaned, "[^A-Z0-9$. ]+", " ");
        cleaned = cleaned.Replace("QTD.", "QTD", StringComparison.Ordinal)
            .Replace("R$", "R$", StringComparison.Ordinal);
        cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();

        return cleaned;
    }

    public string ToDisplayName(string canonicalName)
        => canonicalName switch
        {
            "CODIGO" => "CÓDIGO",
            "DESCRICAO_ITEM" => "DESCRIÇÃO DO ITEM",
            "UNIDADE" => "UNID",
            "QTD_LICITADA" => "QTD. LICITADA",
            "QTD_ADITIVADA" => "QTD. ADITIVADA",
            "QTD_CONTRATADA" => "QTD. CONTRATADA",
            "QTD_EM_COMPRA" => "QTD. EM COMPRA",
            "QTD_COMPRADA" => "QTD. COMPRADA",
            "QTD_EXPIRADA" => "QTD. EXPIRADA",
            "QTD_SALDO" => "QTD. SALDO",
            "VALOR_ITEM" => "VALOR ITEM (R$)",
            "VALOR_SALDO" => "VALOR SALDO (R$)",
            _ => canonicalName.Replace("_", " ", StringComparison.Ordinal)
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
