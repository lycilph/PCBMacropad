using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using MacropadConfigurator.DTO;
using NLog;
using System.Runtime.InteropServices;

namespace MacropadConfigurator.Services;

public class CommunicationService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    
    private HidDevice? device = null;
    private HidStream? stream = null;

    private int reportLength = 0;
    private ReportDescriptor? reportDescriptor = null;
    private HidDeviceInputReceiver? inputReceiver = null;

    private readonly List<byte> receivedDataBuffer = [];
    private int expectedBytesToReceive = 0;

    public event EventHandler<MacropadConfigurationDTO>? ConfigurationLoaded;

    public Task<bool> FindMacropad()
    {
        return Task.Run(() =>
        {
            var retries = 5;
            while (retries-- > 0 && device == null)
            {
                logger.Debug("Getting HID devices...");
                device = DeviceList
                    .Local
                    .GetHidDevices(Constants.VendorId, Constants.ProductId)
                    .FirstOrDefault();

                if (device == null && retries > 0)
                {
                    logger.Debug("No device found, waiting before retrying");
                    Task.Delay(200).Wait();
                }
            }

            if (device != null)
            {
                logger.Debug($"Found Macropad: {device.GetFriendlyName()}");
                return true;
            }
            else
            {
                logger.Debug($"Device with VID=0x{Constants.VendorId:X4}, PID=0x{Constants.ProductId:X4} not found");
                return false;
            }
        });
    }

    public void LoadConfiguration()
    {
        if (device == null)
        {
            logger.Error("Cannot load configuration: No device found");
            return;
        }

        if (device.TryOpen(out stream))
        {
            logger.Info("Stream opened successfully");

            reportLength = device.GetMaxOutputReportLength();
            reportDescriptor = device.GetReportDescriptor();
            inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();

            inputReceiver.Received += InputReceiver_Received;
            inputReceiver.Stopped += (s, e) => logger.Info("Input receiver stopped");
            inputReceiver.Start(stream);

            while (!inputReceiver.IsRunning)
            {
                // Wait for the receiver to start
                Task.Delay(250).Wait();
                logger.Info("Waiting for input receiver to start...");
            }

            var requestPacket = new byte[] { Constants.RawHidInputReportId, Constants.CMD_PC_GET_CONFIG };
            logger.Info("Sending data request command to Arduino...");
            stream.Write(requestPacket);
        }
    }

    private void InputReceiver_Received(object? sender, EventArgs e)
    {
        if (sender == null) return; // No sender, nothing to process

        var inputReceiver = (HidDeviceInputReceiver)sender;
        var packet = new byte[reportLength];

        // While there are reports in the queue, process them.
        while (inputReceiver.TryRead(packet, 0, out _/*var report*/))
        {
            // The first byte is the Report ID. This is not set by the HID-Project library and is always 0 for RawHID.
            if (packet[0] != Constants.RawHidInputReportId)
                continue; // Not the report we are looking for

            // The second byte is our actual command
            byte command = packet[1];

            if (command == Constants.CMD_ARDUINO_SEND_CONFIG)
            {
                receivedDataBuffer.Clear();
                expectedBytesToReceive = packet[2] | (packet[3] << 8);

                // The rest of the report is the first chunk of data
                int dataLength = packet.Length - 4;
                receivedDataBuffer.AddRange(packet.Skip(4).Take(dataLength));

                logger.Info($"[Listener] Received START_RESPONSE. Expecting {expectedBytesToReceive} bytes.");
            }
            else if (command == Constants.CMD_ARDUINO_CONFIG_DATA && expectedBytesToReceive > 0)
            {
                int dataLength = packet.Length - 2;
                receivedDataBuffer.AddRange(packet.Skip(2).Take(dataLength));
            }

            // Check if the transfer is complete
            if (expectedBytesToReceive > 0 && receivedDataBuffer.Count >= expectedBytesToReceive)
            {
                logger.Info("--- Arduino->PC Transfer Complete! ---");
                logger.Info($"Successfully received {receivedDataBuffer.Count} bytes.");

                logger.Info($"Parsing config");
                var config = ByteArrayToStruct<MacropadConfigurationDTO>(receivedDataBuffer.ToArray());
                PrintConfig(config);
                OnConfigurationLoaded(config);

                // Reset for the next transfer and re-draw the menu prompt
                expectedBytesToReceive = 0;
                receivedDataBuffer.Clear();
            }
        }
    }

    public void SaveConfiguration(MacropadConfigurationDTO config)
    {
        if (device == null)
        {
            logger.Error("Cannot load configuration: No device found");
            return;
        }

        if (device.TryOpen(out stream))
        {
            logger.Info("Stream opened successfully");
            
            reportLength = device.GetMaxOutputReportLength();

            var configData = StructToByteArray(config);
            logger.Info($"Sending {configData.Length} bytes to the device.");

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

            logger.Info($"Sent first chunk of {firstChunkSize} bytes.");

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

                logger.Info($"Sent data chunk of {firstChunkSize} bytes.");

                bytesSent += chunkSize;
                Thread.Sleep(20);
            }
            logger.Info("Finished sending all data packets.");

            stream?.Close();
        }
        device = null;
    }

    public void ResetConfiguration()
    {
        if (device == null)
        {
            logger.Error("Cannot load configuration: No device found");
            return;
        }

        if (device.TryOpen(out stream))
        {
            logger.Info("Stream opened successfully");

            reportLength = device.GetMaxOutputReportLength();

            var requestPacket = new byte[] { Constants.RawHidInputReportId, Constants.CMD_PC_RESET_CONFIG };
            logger.Info("Sending config reset request command to Arduino...");
            stream.Write(requestPacket);

            stream?.Close();
        }
        device = null;
    }

    public void OnConfigurationLoaded(MacropadConfigurationDTO config)
    {
        stream?.Close();
        device = null;
        ConfigurationLoaded?.Invoke(this, config);
    }

    private static byte[] StructToByteArray<T>(T obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
#pragma warning disable CS8607 // A possible null value may not be used for a type marked with [NotNull] or [DisallowNull]
        Marshal.StructureToPtr(obj, ptr, true);
#pragma warning restore CS8607 // A possible null value may not be used for a type marked with [NotNull] or [DisallowNull]
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
#pragma warning disable CS8605 // Unboxing a possibly null value.
        obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
#pragma warning restore CS8605 // Unboxing a possibly null value.
        Marshal.FreeHGlobal(ptr);
        return obj;
    }

    private static void PrintConfig(MacropadConfigurationDTO config)
    {
        foreach (var layer in config.layers)
        {
            logger.Info($"Layer: {layer.name}, Enabled: {layer.isEnabled}");
            foreach (var button in layer.buttons)
            {
                logger.Info($"  Key: {button.key}, Modifier: {button.modifier}, Text: {button.text}");
            }
        }
    }
}
