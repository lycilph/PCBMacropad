using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace MacropadConfigurator.Services;

public class KeyboardHook
{
    // Win32 API Constants
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    // Delegate and P/Invoke declarations
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // Private fields
    private readonly LowLevelKeyboardProc proc;
    private IntPtr hookID = IntPtr.Zero;

    // Event to be raised when a shortcut is pressed
    public event Action<Key, ModifierKeys> ShortcutPressed = null!;

    public KeyboardHook()
    {
        proc = HookCallback; // The callback must be saved to this proc variable, as passing it directly to SetHook will result in a crash
        hookID = SetHook(proc);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule == null)
                throw new InvalidOperationException("Couldn't find a main module in the current process");

            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Key key = KeyInterop.KeyFromVirtualKey(vkCode);

            // Get current modifier keys
            ModifierKeys modifiers = Keyboard.Modifiers;

            // Raise the event
            ShortcutPressed?.Invoke(key, modifiers);
        }
        return CallNextHookEx(hookID, nCode, wParam, lParam);
    }

    // Implement IDisposable to ensure the hook is released
    public void Dispose()
    {
        if (hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }

    ~KeyboardHook()
    {
        Dispose();
    }
}
