namespace MacropadConfigurator.DTO;

public static class Constants
{
    public const int VendorId = 0x1B4F;
    public const int ProductId = 0x9206;

    public const int NumButtons = 9;
    public const int NumLayers = 3;
    public const int ButtonTextLength = 6;
    public const int LayerTextLength = 12;

    // The Report ID for RawHID input from the HID-Project library
    public const byte RawHidInputReportId = 0;

    public const byte CMD_PC_GET_CONFIG = (byte)'G';
    public const byte CMD_PC_SET_CONFIG = (byte)'S';
    public const byte CMD_PC_CONFIG_DATA = (byte)'D';
    public const byte CMD_PC_RESET_CONFIG = (byte)'R';

    public const byte CMD_ARDUINO_SEND_CONFIG = (byte)'C';
    public const byte CMD_ARDUINO_CONFIG_DATA = (byte)'D';
}