using Microsoft.Win32;

namespace PdfParaExcelApp.Services;

public class FileDialogService : IFileDialogService
{
    public string? PickPdfFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Arquivos PDF (*.pdf)|*.pdf",
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickOutputFile(string suggestedName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
            FileName = suggestedName,
            AddExtension = true,
            DefaultExt = ".xlsx"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
