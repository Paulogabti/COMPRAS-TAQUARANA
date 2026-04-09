using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfParaExcelApp.Models;

public partial class ColumnDefinition : ObservableObject
{
    public required string OriginalName { get; init; }
    public required string CanonicalName { get; init; }
    public required string DisplayName { get; init; }
    public double XStart { get; init; }
    public double XEnd { get; set; }

    [ObservableProperty]
    private bool _isSelected = true;
}
