using System.Runtime.InteropServices;

namespace MacropadConfigurator.DTO;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct MacropadLayerDTO
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.LayerTextLength)]
    public string name; // Corresponds to char[12]

    [MarshalAs(UnmanagedType.U1)]
    public bool isEnabled; // Corresponds to bool (U1 forces 1-byte)

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NumButtons)]
    public MacropadButtonDTO[] actions; // Corresponds to KeyAction[9]
}