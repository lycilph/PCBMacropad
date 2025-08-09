using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MacropadConfigurator.Scripting;

public class ScriptHost(App app)
{
    public void ShowMessageBox(string message)
    {
        MessageBox.Show(message);
    }

    public int GetMonitorCount()
    {
        return WindowApiHelper.GetSystemMetrics(WindowApiHelper.SM_CMONITORS);
    }

    public void ExitApplication()
    {
        app.Shutdown();
    }

    public void ToggleApplicationWindow()
    {
        if (app.MainWindow != null)
        {
            if (app.MainWindow.Visibility == Visibility.Visible)
            {
                app.MainWindow.Hide();
            }
            else
            {
                app.MainWindow.Show();
                app.MainWindow.Activate();
            }
        }
    }

    public Process Run(string path)
    {
        return Process.Start(path);
    }

    public void MoveWindow(string path, int x, int y, int width, int height)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var hWnd = WindowApiHelper.FindWindowByProcessName(filename);
        if (hWnd != IntPtr.Zero)
            WindowApiHelper.MoveWindow(hWnd, x, y, width, height, true);
    }

    public void MoveWindow(string path, int x, int y)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var hWnd = WindowApiHelper.FindWindowByProcessName(filename);

        if (hWnd != IntPtr.Zero && WindowApiHelper.GetWindowRect(hWnd, out RECT currentRect))
        {
            // Calculate the current width and height from the RECT struct.
            int width = currentRect.Right - currentRect.Left;
            int height = currentRect.Bottom - currentRect.Top;

            // Call the original MoveWindow function with the new position but old size.
            WindowApiHelper.MoveWindow(hWnd, x, y, width, height, true);
        }
    }
}
