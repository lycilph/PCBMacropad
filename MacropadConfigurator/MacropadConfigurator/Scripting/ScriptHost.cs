using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;

namespace MacropadConfigurator.Scripting;

public class ScriptHost(App app)
{
    public void ShowMessageBox(string message)
    {
        MessageBox.Show(message);
    }

    public void ShowNotification(string message)
    {
        WeakReferenceMessenger.Default.Send(new ShowToastMessage(message));
    }

    public string GetAllOutputDevices()
    {
        string psScriptPath = @"3rdParty\GetAllOutputDevices.ps1";
        string output = string.Empty;

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{psScriptPath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        return output;
    }

    public void SetCurrentOutputDevice(string device_name)
    {
        string psScriptPath = @"3rdParty\SetOutputDevice.ps1";

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{psScriptPath}\" -DeviceName \"{device_name}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                ShowMessageBox("Error: " + error);
        }
    }

    public string GetCurrentOutputDevice()
    {
        string psScriptPath = @"3rdParty\GetOutputDevice.ps1";
        string output = string.Empty;

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{psScriptPath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        return output;
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

    public Process? Run(string path, bool useShellExecute = true)
    {
        var info = new ProcessStartInfo(path) { UseShellExecute = useShellExecute };

        if (info == null)
            throw new ArgumentException($"Cannot run [{path}]");
        else
            return Process.Start(info);
    }

    public bool WindowExist(string path)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var hWnd = WindowApiHelper.FindWindowByProcessName(filename);
        return hWnd != IntPtr.Zero;
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

    public void Sleep(int milliseconds) => Thread.Sleep(milliseconds);
}
