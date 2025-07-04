using System.Windows;

namespace MacropadConfigurator.Models;

public class ScriptHost
{
    public void ShowMessageBox(string message)
    {
        MessageBox.Show(message);
    }
}
