using System.Diagnostics;
using System.IO;
using System.Windows;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.Models;

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
