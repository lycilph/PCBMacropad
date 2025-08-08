using System.Runtime.InteropServices;

namespace MacropadConfigurator.DTO;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct MacropadButtonDTO
{
    public byte key; // Corresponds to uint8_t
    public ushort modifier; // Corresponds to uint16_t

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.ButtonTextLength)]
    public string text; // Corresponds to char[6]
}