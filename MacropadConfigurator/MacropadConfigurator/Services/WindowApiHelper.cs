using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MacropadConfigurator.Services;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public static class WindowApiHelper
{
    public const int SM_CMONITORS = 80;
    // P/Invoke declarations
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    /// <summary>
    /// Finds the main window handle for a process by its name, enumerating all desktop windows.
    /// This is more robust than Process.MainWindowHandle.
    /// </summary>
    public static IntPtr FindWindowByProcessName(string processName)
    {
        var matchingWindow = IntPtr.Zero;

        // 1. Get all process IDs for the given process name. Using a HashSet for fast lookups.
        var pids = Process.GetProcessesByName(processName)
                          .Select(p => (uint)p.Id)
                          .ToHashSet();

        if (pids.Count == 0) return IntPtr.Zero;

        // 2. Enumerate all top-level windows
        EnumWindows((hWnd, lParam) =>
        {
            // 3. For each window, check if it's visible and has a title.
            if (!IsWindowVisible(hWnd) || GetWindowTextLength(hWnd) == 0)
            {
                return true; // Continue enumeration
            }

            // 4. Get the process ID for the window.
            GetWindowThreadProcessId(hWnd, out uint windowPid);

            // 5. If the PID matches one of our Chrome processes, we've found our window.
            if (pids.Contains(windowPid))
            {
                matchingWindow = hWnd;
                return false; // Stop enumeration
            }

            return true; // Continue enumeration
        }, IntPtr.Zero);

        return matchingWindow;
    }
}
