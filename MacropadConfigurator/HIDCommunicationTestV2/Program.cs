namespace HIDCommunicationTestV2;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using HidSharp;

// --- Constants ---
// IMPORTANT: Update these to match your Arduino's VID and PID!
public static class Constants
{
    public const int VendorId = 0x1B4F;
    public const int ProductId = 0x9206;

    public const int NumButtons = 9;
    public const int NumLayers = 3;
    public const int ButtonTextLength = 6;
    public const int LayerTextLength = 12;

    public const byte CmdPcGetConfig = (byte)'G';
    public const byte CmdPcSetConfig = (byte)'S';
    public const byte CmdPcNextPacket = (byte)'N';

    public const byte CmdArduinoReadyToSend = (byte)'R';
    public const byte CmdArduinoAck = (byte)'A';
    public const byte CmdArduinoError = (byte)'E';
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


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Arduino Macropad Communicator ---");

        var device = FindMacropad();
        if (device == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Device with VID=0x{Constants.VendorId:X4}, PID=0x{Constants.ProductId:X4} not found.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Found Macropad: {device.GetFriendlyName()}");
        Console.ResetColor();

        using (var stream = device.Open())
        {
            stream.ReadTimeout = 2000; // 2 second timeout

            while (true)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Get Configuration from Macropad");
                Console.WriteLine("2. Send New Configuration to Macropad");
                Console.WriteLine("3. Exit");
                Console.Write("> ");

                var choice = Console.ReadKey().KeyChar;
                Console.WriteLine();

                int reportSize = device.GetMaxOutputReportLength();
                switch (choice)
                {
                    case '1':
                        GetConfigFromDevice(stream, reportSize);
                        break;
                    case '2':
                        SendConfigToDevice(stream);
                        break;
                    case '3':
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }

    private static void GetConfigFromDevice(HidStream stream, int reportSize)
    {
        Console.WriteLine("\nRequesting config from device...");

        // Send the 'Get' command
        var outputBuffer = new byte[reportSize];
        outputBuffer[0] = 0x00; // Report ID
        outputBuffer[1] = Constants.CmdPcGetConfig;
        try
        {
            stream.Write(outputBuffer);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error writing to device: {ex.Message}");
            Console.ResetColor();
            return;
        }

        try
        {
            // Step 1: Wait for the Arduino to signal it's 'Ready'
            Console.WriteLine("Waiting for 'Ready' signal from device...");
            var readyPacket = stream.Read();

            if (readyPacket.Length < 2 || readyPacket[1] != Constants.CmdArduinoReadyToSend)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Expected 'Ready' signal (0x{Constants.CmdArduinoReadyToSend:X2}) but got something else.");
                if (readyPacket.Length >= 2) Console.WriteLine($"Received: 0x{readyPacket[1]:X2}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("'Ready' signal received. Requesting packets...");

            // Step 2: Request each packet one by one
            var configSizeBytes = Marshal.SizeOf<Layer>() * Constants.NumLayers;
            var fullConfigBuffer = new byte[configSizeBytes];
            var bytesRead = 0;

            // Prepare the 'Next' command packet. We can reuse this.
            byte[] nextCommand = { 0x00, Constants.CmdPcNextPacket };

            while (bytesRead < configSizeBytes)
            {
                // Ask for the next packet by sending the minimal 'Next' command report.
                stream.Write(nextCommand);

                // Read the data packet the Arduino sends in response
                var dataPacket = stream.Read();
                if (dataPacket.Length <= 1) // Must contain more than just the Report ID
                {
                    Console.WriteLine("Received an empty or invalid data packet. Aborting.");
                    break;
                }

                int dataOffset = 1; // Skip Report ID
                int dataLength = dataPacket.Length - dataOffset;

                Console.WriteLine($"  -> Received packet with {dataLength} data bytes.");

                string hexPayload = string.Join(" ", dataPacket.Skip(dataOffset).Select(b => b.ToString("X2")));
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"     Bytes: {hexPayload}");
                Console.ResetColor();

                var bytesToCopy = Math.Min(dataLength, configSizeBytes - bytesRead);
                Buffer.BlockCopy(dataPacket, dataOffset, fullConfigBuffer, bytesRead, bytesToCopy);
                bytesRead += bytesToCopy;

                Console.WriteLine($"     Total bytes read: {bytesRead}/{configSizeBytes}");
            }

            // Final verification
            if (bytesRead == configSizeBytes)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSuccessfully received full configuration!");
                Console.ResetColor();
                var layers = DeserializeConfig(fullConfigBuffer);
                PrintConfig(layers);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nIncomplete data received. Expected {configSizeBytes}, but got {bytesRead}.");
                Console.ResetColor();
            }
        }
        catch (TimeoutException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nTimeout waiting for response from device. This may indicate a problem on the Arduino side.");
            Console.ResetColor();
        }

        //try
        //{
        //    // Step 1: Read the initial command packet from the Arduino
        //    Console.WriteLine("Waiting for command packet from device...");
        //    var commandPacket = stream.Read(); // Timeout is 2s

        //    // Packet must have at least 2 bytes: ReportID(0) and Command('C')
        //    if (commandPacket.Length < 2 || commandPacket[1] != Constants.CmdArduinoSendConfig)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine($"Error: Expected config start command (0x{Constants.CmdArduinoSendConfig:X2}) but got something else.");
        //        if (commandPacket.Length >= 2) Console.WriteLine($"Received: 0x{commandPacket[1]:X2}");
        //        Console.ResetColor();
        //        return;
        //    }

        //    Console.WriteLine("Command packet received. Now reading data packets...");

        //    // Step 2: Read the data packets in a loop
        //    var configSizeBytes = Marshal.SizeOf<Layer>() * Constants.NumLayers;
        //    var fullConfigBuffer = new byte[configSizeBytes];
        //    var bytesRead = 0;

        //    while (bytesRead < configSizeBytes)
        //    {
        //        var dataPacket = stream.Read();
        //        if (dataPacket.Length == 0)
        //        {
        //            Console.WriteLine("Read timed out while waiting for data packet.");
        //            break;
        //        }

        //        // The data starts at index 1 (after the Report ID)
        //        int dataOffset = 1;
        //        int dataLength = dataPacket.Length - dataOffset;

        //        // This is for our debug info
        //        Console.WriteLine($"  -> Received packet with {dataLength} data bytes.");

        //        string hexPayload = string.Join(" ", dataPacket.Skip(dataOffset).Select(b => b.ToString("X2")));
        //        Console.ForegroundColor = ConsoleColor.DarkGray;
        //        Console.WriteLine($"     Bytes: {hexPayload}");
        //        Console.ResetColor();

        //        var bytesToCopy = Math.Min(dataLength, configSizeBytes - bytesRead);
        //        Buffer.BlockCopy(dataPacket, dataOffset, fullConfigBuffer, bytesRead, bytesToCopy);
        //        bytesRead += bytesToCopy;

        //        Console.WriteLine($"     Total bytes read: {bytesRead}/{configSizeBytes}");
        //    }

        //    if (bytesRead == configSizeBytes)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine("\nSuccessfully received full configuration!");
        //        Console.ResetColor();

        //        var layers = DeserializeConfig(fullConfigBuffer);
        //        PrintConfig(layers);
        //    }
        //    else
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine($"\nIncomplete data received. Expected {configSizeBytes}, but got {bytesRead}.");
        //        Console.ResetColor();
        //    }

        //}
        //catch (TimeoutException)
        //{
        //    Console.ForegroundColor = ConsoleColor.Red;
        //    Console.WriteLine("\nTimeout waiting for response from device.");
        //    Console.ResetColor();
        //}

        //try
        //{
        //    while (bytesRead < configSizeBytes)
        //    {
        //        // inputBuffer will contain [0, Command, Data...] or [0, Data...]
        //        var inputBuffer = stream.Read();
        //        if (inputBuffer.Length == 0)
        //        {
        //            Console.WriteLine("Read timed out.");
        //            break;
        //        }

        //        for (int i = 0; i < inputBuffer.Length; i++)
        //        {
        //            Console.Write($"0x{inputBuffer[i]:X2} ");
        //        }

        //        // The first byte is the report ID (0), so we ignore it.
        //        int dataOffset;
        //        int dataLength;

        //        if (bytesRead == 0) // This is the first packet, it contains the command
        //        {
        //            // Check command byte at index 1
        //            if (inputBuffer.Length < 2 || inputBuffer[1] != Constants.CmdArduinoSendConfig) // <-- CHANGED
        //            {
        //                Console.ForegroundColor = ConsoleColor.Red;
        //                // <-- CHANGED: Check index 1 for the command
        //                Console.WriteLine($"Error: Expected config start command (0x{Constants.CmdArduinoSendConfig:X2}) but got 0x{inputBuffer[1]:X2}");
        //                Console.ResetColor();
        //                return;
        //            }
        //            // Data starts at index 2, after Report ID and Command
        //            dataOffset = 2; // <-- CHANGED
        //        }
        //        else // Subsequent packets are just data
        //        {
        //            // Data starts at index 1, after Report ID
        //            dataOffset = 1; // <-- CHANGED
        //        }

        //        dataLength = inputBuffer.Length - dataOffset;
        //        var bytesToCopy = Math.Min(dataLength, configSizeBytes - bytesRead);
        //        Buffer.BlockCopy(inputBuffer, dataOffset, fullConfigBuffer, bytesRead, bytesToCopy); // <-- CHANGED
        //        bytesRead += bytesToCopy;

        //        Console.WriteLine($"Bytes read: {bytesRead}/{configSizeBytes} (Current packet size: {inputBuffer.Length})");
        //    }

        //    if (bytesRead == configSizeBytes)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine("Successfully received configuration!");
        //        Console.ResetColor();

        //        var layers = DeserializeConfig(fullConfigBuffer);
        //        PrintConfig(layers);
        //    }
        //    else
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine($"Incomplete data received. Expected {configSizeBytes}, got {bytesRead}.");
        //        Console.ResetColor();
        //    }

        //}
        //catch (TimeoutException)
        //{
        //    Console.ForegroundColor = ConsoleColor.Red;
        //    Console.WriteLine("Timeout waiting for response from device.");
        //    Console.ResetColor();
        //}
    }

    private static void SendConfigToDevice(HidStream stream)
    {
        Console.WriteLine("\nCreating and sending a new configuration...");

        // Create some new example data
        var layers = new Layer[Constants.NumLayers];
        layers[0] = new Layer
        {
            name = "C# Layer 1",
            isEnabled = true,
            actions = new KeyAction[Constants.NumButtons]
        };
        // Set some actions for layer 0
        layers[0].actions[0] = new KeyAction { key = 0x06, modifier = 0x02, text = "CtrlC" }; // Ctrl+C (Key C = 0x06, Left Ctrl = 0x01, but HID-Project uses 0x02 for some reason)
        layers[0].actions[1] = new KeyAction { key = 0x19, modifier = 0x02, text = "CtrlV" }; // Ctrl+V (Key V = 0x19)

        layers[1] = new Layer { name = "C# Media", isEnabled = true, actions = new KeyAction[Constants.NumButtons] };
        layers[1].actions[0] = new KeyAction { key = 0, modifier = 0, text = "Mute" }; // Placeholder for media key

        layers[2] = new Layer { name = "C# Disabled", isEnabled = false, actions = new KeyAction[Constants.NumButtons] };


        // Serialize this data into a byte array
        byte[] serializedData = SerializeConfig(layers);

        // Prepare the buffer to send. It's the command byte + the data.
        byte[] sendBuffer = new byte[1 + serializedData.Length];
        sendBuffer[0] = Constants.CmdPcSetConfig;
        Buffer.BlockCopy(serializedData, 0, sendBuffer, 1, serializedData.Length);

        try
        {
            // HidSharp handles splitting the large buffer into 64-byte chunks
            stream.Write(sendBuffer);
            Console.WriteLine("New configuration sent. Waiting for acknowledgement...");

            // Wait for ACK or ERROR
            var response = stream.Read();
            if (response.Length > 0)
            {
                if (response[0] == Constants.CmdArduinoAck)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success! Macropad acknowledged the update.");
                    Console.ResetColor();
                }
                else if (response[0] == Constants.CmdArduinoError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Macropad reported an error during the update.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Received unknown response from device: 0x{response[0]:X2}");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error communicating with device: {ex.Message}");
            Console.ResetColor();
        }
    }


    private static HidDevice FindMacropad()
    {
        return DeviceList.Local.GetHidDevices(vendorID: Constants.VendorId, productID: Constants.ProductId).FirstOrDefault();
    }

    private static byte[] SerializeConfig(Layer[] layers)
    {
        int layerSize = Marshal.SizeOf<Layer>();
        int totalSize = layerSize * layers.Length;
        byte[] buffer = new byte[totalSize];

        IntPtr ptr = Marshal.AllocHGlobal(totalSize);
        try
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Marshal.StructureToPtr(layers[i], ptr + (i * layerSize), false);
            }
            Marshal.Copy(ptr, buffer, 0, totalSize);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return buffer;
    }

    private static Layer[] DeserializeConfig(byte[] buffer)
    {
        var layers = new Layer[Constants.NumLayers];
        int layerSize = Marshal.SizeOf<Layer>();

        IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
        try
        {
            Marshal.Copy(buffer, 0, ptr, buffer.Length);
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = Marshal.PtrToStructure<Layer>(ptr + (i * layerSize));
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return layers;
    }

    private static void PrintConfig(Layer[] layers)
    {
        Console.WriteLine("\n--- PARSED CONFIGURATION ---");
        foreach (var layer in layers)
        {
            Console.WriteLine($"Layer: '{layer.name}', Enabled: {layer.isEnabled}");
            if (layer.actions != null)
            {
                for (int i = 0; i < layer.actions.Length; i++)
                {
                    var action = layer.actions[i];
                    Console.WriteLine($"  Btn {i}: '{action.text}', Key: 0x{action.key:X2}, Mod: 0x{action.modifier:X2}");
                }
            }
        }
        Console.WriteLine("---------------------------\n");
    }
}
