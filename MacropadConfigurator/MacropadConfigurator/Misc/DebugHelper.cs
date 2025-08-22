using System.Diagnostics;

namespace MacropadConfigurator.Misc;

public static class DebugHelper
{
    [Conditional("DEBUG")]
    private static void IsDebugCheck(ref bool isDebug)
    {
        isDebug = true;
    }

    public static bool IsDebug()
    {
        var is_debug = false;
        IsDebugCheck(ref is_debug);
        return is_debug;
    }
}
