using System.Runtime.InteropServices;

namespace MacropadConfigurator.DTO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MacropadConfigurationDTO
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public MacropadLayerDTO[] layers;
}