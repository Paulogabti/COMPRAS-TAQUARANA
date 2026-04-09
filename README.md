# PDF para Excel (WPF .NET 8)

Aplicação desktop para Windows que lê tabelas de PDF (padrão de relatório de saldo/itens), detecta colunas automaticamente e exporta para Excel `.xlsx` formatado.

## Stack
- C# / .NET 8
- WPF + MVVM (CommunityToolkit.Mvvm)
- Parsing PDF com UglyToad.PdfPig
- Exportação Excel com ClosedXML

## Funcionalidades
- Seleção de PDF por botão e por arrastar/soltar.
- Detecção automática de cabeçalhos e colunas.
- Normalização de nomes equivalentes (`DESCRIÇÃO DO ITEM`, `DESCRIÇÃO ITEM`, etc.).
- Seleção de colunas por checkbox.
- Prévia das primeiras linhas extraídas.
- Exportação `.xlsx` com:
  - cabeçalho em negrito
  - autofiltro
  - congelamento da primeira linha
  - bordas leves
  - quebra de linha em células
  - largura maior para coluna de descrição
- Persistência de último PDF e último destino exportado.

## Estratégia de parser
1. Ler palavras com coordenadas X/Y via PdfPig.
2. Agrupar palavras em linhas por proximidade de Y.
3. Detectar linha de cabeçalho por palavras-chave (`CÓDIGO`, `DESCRIÇÃO`, `UNID`, `QTD`, `VALOR`).
4. Detectar colunas por posição X dos títulos.
5. Mapear palavras de cada linha para colunas pelo intervalo horizontal.
6. Detectar continuação de item quando não há novo código e mesclar com a linha anterior.
7. Ignorar cabeçalhos repetidos, paginação e textos de totalização/rodapé.
8. Aplicar fallback textual para complementar descrição quando dados posicionais vierem incompletos.

## Estrutura
- `src/PdfParaExcelApp/Views`
- `src/PdfParaExcelApp/ViewModels`
- `src/PdfParaExcelApp/Models`
- `src/PdfParaExcelApp/Services`
- `src/PdfParaExcelApp/Parsers`
- `src/PdfParaExcelApp/Helpers`
- `tests/PdfParaExcelApp.Tests`

## Build local
```bash
dotnet restore
dotnet build PdfParaExcelApp.sln -c Release
```

## Testes
```bash
dotnet test tests/PdfParaExcelApp.Tests/PdfParaExcelApp.Tests.csproj -c Release
```

## Publicação single-file `.exe` (Windows)
```bash
dotnet publish src/PdfParaExcelApp/PdfParaExcelApp.csproj \
  -c Release \
  -r win-x64 \
  -p:PublishSingleFile=true \
  -p:SelfContained=true \
  -p:PublishReadyToRun=true
```

Saída esperada em:
- `src/PdfParaExcelApp/bin/Release/net8.0-windows/win-x64/publish/PdfParaExcelApp.exe`

## Observação
O PDF `ATA JARDINAGEM.pdf` no repositório pode ser usado como referência de validação do parser.
