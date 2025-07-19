using System.Windows.Input;

namespace MacropadConfigurator.Services;

public static class KeyParser
{
    // These must match the definitions in HID-Project.h
    private const ushort MOD_LEFT_CTRL_BIT = (1 << 8);
    private const ushort MOD_LEFT_SHIFT_BIT = (1 << 9);
    private const ushort MOD_LEFT_ALT_BIT = (1 << 10);
    //private const ushort MOD_LEFT_GUI_BIT = (1 << 11);
    private const ushort MOD_RIGHT_CTRL_BIT = (1 << 12);
    private const ushort MOD_RIGHT_SHIFT_BIT = (1 << 13);
    private const ushort MOD_RIGHT_ALT_BIT = (1 << 14);
    //private const ushort MOD_RIGHT_GUI_BIT = (1 << 15);

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

    // This dictionary maps WPF Key enums to their USB HID Usage ID (keyboard page 0x07).
    // Based on https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf (pages 53-59)
    private static readonly Dictionary<Key, byte> _keyToHidMap = new Dictionary<Key, byte>
    {
        // Letters
        { Key.A, 0x04 }, { Key.B, 0x05 }, { Key.C, 0x06 }, { Key.D, 0x07 },
        { Key.E, 0x08 }, { Key.F, 0x09 }, { Key.G, 0x0A }, { Key.H, 0x0B },
        { Key.I, 0x0C }, { Key.J, 0x0D }, { Key.K, 0x0E }, { Key.L, 0x0F },
        { Key.M, 0x10 }, { Key.N, 0x11 }, { Key.O, 0x12 }, { Key.P, 0x13 },
        { Key.Q, 0x14 }, { Key.R, 0x15 }, { Key.S, 0x16 }, { Key.T, 0x17 },
        { Key.U, 0x18 }, { Key.V, 0x19 }, { Key.W, 0x1A }, { Key.X, 0x1B },
        { Key.Y, 0x1C }, { Key.Z, 0x1D },

        // Numbers (Top Row)
        { Key.D1, 0x1E }, { Key.D2, 0x1F }, { Key.D3, 0x20 }, { Key.D4, 0x21 },
        { Key.D5, 0x22 }, { Key.D6, 0x23 }, { Key.D7, 0x24 }, { Key.D8, 0x25 },
        { Key.D9, 0x26 }, { Key.D0, 0x27 },

        // Function Keys
        { Key.F1, 0x3A }, { Key.F2, 0x3B }, { Key.F3, 0x3C }, { Key.F4, 0x3D },
        { Key.F5, 0x3E }, { Key.F6, 0x3F }, { Key.F7, 0x40 }, { Key.F8, 0x41 },
        { Key.F9, 0x42 }, { Key.F10, 0x43 }, { Key.F11, 0x44 }, { Key.F12, 0x45 },
        { Key.F13, 0x68 }, { Key.F14, 0x69 }, { Key.F15, 0x6A }, { Key.F16, 0x6B },
        { Key.F17, 0x6C }, { Key.F18, 0x6D }, { Key.F19, 0x6E }, { Key.F20, 0x6F },
        { Key.F21, 0x70 }, { Key.F22, 0x71 }, { Key.F23, 0x72 }, { Key.F24, 0x73 },

        // Special Keys
        { Key.Enter, 0x28 },
        { Key.Escape, 0x29 },
        { Key.Back, 0x2A }, // Backspace
        { Key.Tab, 0x2B },
        { Key.Space, 0x2C },
        { Key.OemMinus, 0x2D },
        { Key.OemPlus, 0x2E },
        { Key.OemOpenBrackets, 0x2F },
        { Key.OemCloseBrackets, 0x30 },
        { Key.OemPipe, 0x31 },        // \ and |
        { Key.OemSemicolon, 0x33 },   // ; and :
        { Key.OemQuotes, 0x34 },      // ' and "
        { Key.OemTilde, 0x35 },       // ` and ~
        { Key.OemComma, 0x36 },       // , and <
        { Key.OemPeriod, 0x37 },      // . and >
        { Key.OemQuestion, 0x38 },    // / and ?
        { Key.Capital, 0x39 },        // Caps Lock

        // Editing and Navigation
        { Key.Insert, 0x49 },
        { Key.Delete, 0x4C },
        { Key.Home, 0x4A },
        { Key.End, 0x4D },
        { Key.PageUp, 0x4B },
        { Key.PageDown, 0x4E },
        { Key.PrintScreen, 0x46 },
        { Key.Scroll, 0x47 },
        { Key.Pause, 0x48 },
        
        // Arrow Keys
        { Key.Right, 0x4F },
        { Key.Left, 0x50 },
        { Key.Down, 0x51 },
        { Key.Up, 0x52 },

        // Numpad
        { Key.NumLock, 0x53 },
        { Key.Divide, 0x54 },
        { Key.Multiply, 0x55 },
        { Key.Subtract, 0x56 },
        { Key.Add, 0x57 },
        // Numpad Enter is different from main Enter
        // Key.Enter on Numpad is often handled as main Enter, but has its own code: 0x58
        { Key.Decimal, 0x63 },
        { Key.NumPad0, 0x62 },
        { Key.NumPad1, 0x59 }, { Key.NumPad2, 0x5A }, { Key.NumPad3, 0x5B },
        { Key.NumPad4, 0x5C }, { Key.NumPad5, 0x5D }, { Key.NumPad6, 0x5E },
        { Key.NumPad7, 0x5F }, { Key.NumPad8, 0x60 }, { Key.NumPad9, 0x61 }
    };

    private static readonly Dictionary<byte, Key> _hidToKeyMap;

    // --- Mapping for modifier bits to WPF Keys ---
    private static readonly Dictionary<ushort, ModifierKeys> _modifierHidToKeyMap = new()
    {
        { MOD_LEFT_CTRL_BIT, ModifierKeys.Control },
        { MOD_LEFT_SHIFT_BIT, ModifierKeys.Shift },
        { MOD_LEFT_ALT_BIT, ModifierKeys.Alt },
        { MOD_RIGHT_CTRL_BIT, ModifierKeys.Control },
        { MOD_RIGHT_SHIFT_BIT, ModifierKeys.Shift },
        { MOD_RIGHT_ALT_BIT, ModifierKeys.Alt },
    };

    /// <summary>
    /// Static constructor to build the reverse lookup dictionaries.
    /// This runs only once when the class is first accessed.
    /// </summary>
    static KeyParser()
    {
        // Populate the reverse HID-to-Key map from the forward map.
        _hidToKeyMap = new Dictionary<byte, Key>(_keyToHidMap.Count);
        foreach (var pair in _keyToHidMap)
        {
            // In case of duplicate HID codes, this will only keep the last one.
            _hidToKeyMap[pair.Value] = pair.Key;
        }
    }

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

    /// <summary>
    /// Converts a System.Windows.Input.Key to its corresponding USB HID Usage ID.
    /// </summary>
    /// <param name="key">The key to convert.</param>
    /// <returns>The HID Usage ID as a byte, or 0 if the key is not mapped (e.g., modifiers).</returns>
    public static byte KeyToHid(Key key)
    {
        if (_keyToHidMap.TryGetValue(key, out byte hidCode))
        {
            return hidCode;
        }
        // Return 0 for unmapped keys, which is 'HID_KEY_NONE'
        return 0;
    }

    /// <summary>
    /// Converts a USB HID Usage ID to its corresponding System.Windows.Input.Key.
    /// </summary>
    /// <param name="hidCode">The HID Usage ID byte from the Arduino.</param>
    /// <returns>The corresponding WPF Key, or Key.None if not found.</returns>
    public static Key HidToKey(byte hidCode)
    {
        if (_hidToKeyMap.TryGetValue(hidCode, out Key key))
        {
            return key;
        }
        return Key.None;
    }


    public static ModifierKeys HidModifiersToKeys(ushort hidModifiers)
    {
        var mods = ModifierKeys.None;
        foreach (var pair in _modifierHidToKeyMap)
        {
            if ((hidModifiers & pair.Key) != 0)
            {
                mods |= pair.Value;
            }
        }
        return mods;
    }

    //public static ushort KeysToHidModifiers(IEnumerable<Key> modifierKeys)
    //{
    //    ushort hidModifiers = 0;
    //    if (modifierKeys == null) return 0;

    //    var keyToModifierMap = _modifierHidToKeyMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    //    foreach (var key in modifierKeys)
    //    {
    //        if (keyToModifierMap.TryGetValue(key, out ushort bit))
    //        {
    //            hidModifiers |= bit;
    //        }
    //    }
    //    return hidModifiers;
    //}
}