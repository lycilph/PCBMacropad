using System.Windows.Input;

namespace MacropadConfigurator.Services;

public static class KeyParser
{
    // A dictionary to map common, user-friendly names to their official Key enum names.
    private static readonly Dictionary<string, string> _keyNameMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Ctrl", "LeftCtrl" },
        { "LControl", "LeftCtrl" },
        { "RControl", "RightCtrl" },
        { "Alt", "LeftAlt" },
        { "LAlt", "LeftAlt" },
        { "RAlt", "RightAlt" },
        { "Shift", "LeftShift" },
        { "LShift", "LeftShift" },
        { "RShift", "RightShift" },
        { "Win", "LWin" },
        { "LWin", "LWin" },
        { "RWin", "RWin" },
        { "Esc", "Escape" },
        { "Del", "Delete" },
        { "PgUp", "PageUp" },
        { "PgDn", "PageDown" },
        { "Ins", "Insert" },
        { "Apps", "Apps" }, // The context menu key
        { "Scroll", "Scroll" } // Scroll Lock
    };

    /// <summary>
    /// Converts a string representation of a key to its System.Windows.Input.Key enum value.
    /// Supports common aliases.
    /// </summary>
    /// <param name="keyString">The string to parse (e.g., "A", "F5", "Ctrl").</param>
    /// <returns>The corresponding Key enum value, or Key.None if parsing fails.</returns>
    public static Key StringToKey(string keyString)
    {
        if (string.IsNullOrWhiteSpace(keyString))
        {
            return Key.None;
        }

        // Check if the input string is one of our aliases
        if (_keyNameMappings.TryGetValue(keyString, out string mappedName))
        {
            keyString = mappedName;
        }

        try
        {
            // Use Enum.Parse to convert the string to a Key enum.
            // 'true' for ignoreCase.
            return (Key)Enum.Parse(typeof(Key), keyString, true);
        }
        catch (ArgumentException)
        {
            // The string did not match any enum value.
            return Key.None;
        }
    }

}
