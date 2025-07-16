using HidSharp;
using HidSharp.Reports.Input;
using System.Runtime.InteropServices;

namespace HidCommunicationTestV4;

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

    public const byte CMD_ARDUINO_SEND_CONFIG = (byte)'C';
    public const byte CMD_ARDUINO_CONFIG_DATA = (byte)'D';

}

// --- Data Structures ---
// These C# structs MUST EXACTLY match the Arduino structs, including size and packing.

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct KeyAction
{
    public byte key; // Corresponds to uint8_t
    public ushort modifier; // Corresponds to uint16_t

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.ButtonTextLength)]
    public string text; // Corresponds to char[6]
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct Layer
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.LayerTextLength)]
    public string name; // Corresponds to char[12]

    [MarshalAs(UnmanagedType.U1)]
    public bool isEnabled; // Corresponds to bool (U1 forces 1-byte)

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NumButtons)]
    public KeyAction[] actions; // Corresponds to KeyAction[9]
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MacropadConfiguration
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public Layer[] layers;
}

internal class Program
{
    private static int reportLength;
    private static readonly List<byte> receivedDataBuffer = [];
    private static int expectedBytesToReceive = 0;
    
    private static MacropadConfiguration config = new();

    private static readonly object consoleLock = new();

    static void Main(string[] _)
    {
        Console.WriteLine("--- Arduino Macropad Communicator ---");

        var device = FindMacropad();
        if (device == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Device with VID=0x{Constants.VendorId:X4}, PID=0x{Constants.ProductId:X4} not found.");
            Console.Write("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Found Macropad: {device.GetFriendlyName()}");
        Console.ResetColor();

        if (device.TryOpen(out var stream))
        {
            Console.WriteLine("Stream opened successfully.");

            reportLength = device.GetMaxOutputReportLength();
            var reportDescriptor = device.GetReportDescriptor();
            var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();

            Console.WriteLine($"Report length: {reportLength}");

            using (stream)
            {
                // Start a background thread to listen for incoming data
                inputReceiver.Received += InputReceiver_Received;
                inputReceiver.Stopped += (s, e) =>
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine("Input receiver stopped.");
                    }
                };
                inputReceiver.Start(stream);

                // Main menu loop
                while (true)
                {
                    Console.WriteLine("\nSelect an option:");
                    Console.WriteLine("1. Get Configuration from Macropad");
                    Console.WriteLine("2. Send New Configuration to Macropad");
                    Console.WriteLine("3. Change config data");
                    Console.WriteLine("4. Exit");
                    Console.Write("> ");
                    var choice = Console.ReadKey().KeyChar;
                    Console.WriteLine();

                    switch (choice)
                    {
                        case '1':
                            GetConfigFromDevice(stream);
                            break;
                        case '2':
                            SendConfigToDevice(stream);
                            break;
                        case '3':
                            ChangeConfig();
                            break;
                        case '4':
                            return;
                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Failed to open stream to the device.");
        }

        Console.Write("\nPress any key to exit.");
        Console.ReadKey();
    }

    private static HidDevice? FindMacropad()
    {
        return DeviceList.Local.GetHidDevices(vendorID: Constants.VendorId, productID: Constants.ProductId).FirstOrDefault();
    }

    private static void InputReceiver_Received(object? sender, EventArgs e)
    {
        if (sender == null) return; // No sender, nothing to process

        var inputReceiver = (HidDeviceInputReceiver)sender;
        var report = new byte[reportLength];

        // While there are reports in the queue, process them.
        while (inputReceiver.TryRead(report, 0, out _))
        {
            // The first byte is the Report ID. For RawHID, this is usually 3.
            // This is not set by the HID-Project library and is always 0 for RawHID.
            if (report[0] != Constants.RawHidInputReportId)
            {
                continue; // Not the report we are looking for
            }

            PrintPacket(report);

            // The second byte is our actual command
            byte command = report[1];

            lock (consoleLock)
            {
                if (command == Constants.CMD_ARDUINO_SEND_CONFIG)
                {
                    receivedDataBuffer.Clear();
                    expectedBytesToReceive = report[2] | (report[3] << 8);

                    // The rest of the report is the first chunk of data
                    int dataLength = report.Length - 4;
                    receivedDataBuffer.AddRange(report.Skip(4).Take(dataLength));

                    Console.WriteLine($"\n[Listener] Received START_RESPONSE. Expecting {expectedBytesToReceive} bytes.");
                }
                else if (command == Constants.CMD_ARDUINO_CONFIG_DATA && expectedBytesToReceive > 0)
                {
                    int dataLength = report.Length - 2;
                    receivedDataBuffer.AddRange(report.Skip(2).Take(dataLength));
                }

                // Check if the transfer is complete
                if (expectedBytesToReceive > 0 && receivedDataBuffer.Count >= expectedBytesToReceive)
                {
                    Console.WriteLine("\n--- Arduino->PC Transfer Complete! ---");
                    Console.WriteLine($"Successfully received {receivedDataBuffer.Count} bytes.");
                    
                    Console.WriteLine($"Parsing config");
                    config = ByteArrayToStruct<MacropadConfiguration>(receivedDataBuffer.ToArray());
                    PrintConfig(config);

                    Console.WriteLine("\n--------------------------------------");

                    // Reset for the next transfer and re-draw the menu prompt
                    expectedBytesToReceive = 0;
                    receivedDataBuffer.Clear();
                }
            }
        }
    }

    private static void GetConfigFromDevice(HidStream stream)
    {
        var requestPacket = new byte[] { Constants.RawHidInputReportId, Constants.CMD_PC_GET_CONFIG };
        Console.WriteLine("\nSending data request command to Arduino...");
        stream.Write(requestPacket);
    }

    private static void SendConfigToDevice(HidStream stream)
    {
        var configData = StructToByteArray(config);
        PrintPacket(configData);
        Console.WriteLine($"\nSending {configData.Length} bytes to the device.");

        int payloadSize = reportLength - 1;
        int bytesSent = 0;

        // First config Packet
        var packet = new byte[reportLength];
        packet[0] = Constants.RawHidInputReportId;
        packet[1] = Constants.CMD_PC_SET_CONFIG;
        packet[2] = (byte)(configData.Length & 0xFF);
        packet[3] = (byte)((configData.Length >> 8) & 0xFF);

        int firstChunkSize = payloadSize - 3;
        Array.Copy(configData, 0, packet, 4, firstChunkSize);
        stream.Write(packet);

        Console.WriteLine($"Sent first chunk of {firstChunkSize} bytes.");
        PrintPacket(packet);

        bytesSent += firstChunkSize;
        Thread.Sleep(20);

        // DATA Packets
        while (bytesSent < configData.Length)
        {
            packet[0] = Constants.RawHidInputReportId;
            packet[1] = Constants.CMD_PC_CONFIG_DATA;

            int chunkSize = Math.Min(payloadSize - 1, configData.Length - bytesSent);
            Array.Copy(configData, bytesSent, packet, 2, chunkSize);
            stream.Write(packet);

            Console.WriteLine($"Sent data chunk of {firstChunkSize} bytes.");
            PrintPacket(packet);

            bytesSent += chunkSize;
            Thread.Sleep(20);
        }
        Console.WriteLine("Finished sending all data packets.");
    }

    private static void ChangeConfig()
    {
        config.layers[0].name = "Updated";
        config.layers[1].isEnabled = !config.layers[1].isEnabled;
        PrintConfig(config);
    }

    private static void PrintPacket(byte[] packet)
    {
        Console.WriteLine("Packet: ");
        for (int i = 0; i < packet.Length; i++)
        {
            Console.Write($"0x{packet[i]:X2} ");
        }
    }

    private static byte[] StructToByteArray<T>(T obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    private static T ByteArrayToStruct<T>(byte[] arr) where T : struct
    {
        T obj = new T();
        int size = Marshal.SizeOf(obj);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);
        obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
        Marshal.FreeHGlobal(ptr);
        return obj;
    }

    private static void PrintConfig(MacropadConfiguration config)
    {
        foreach (var layer in config.layers)
        {
            Console.WriteLine($"Layer: {layer.name}, Enabled: {layer.isEnabled}");
            foreach (var action in layer.actions)
            {
                Console.WriteLine($"  Key: {action.key}, Modifier: {action.modifier}, Text: {action.text}");
            }
        }
    }
}
