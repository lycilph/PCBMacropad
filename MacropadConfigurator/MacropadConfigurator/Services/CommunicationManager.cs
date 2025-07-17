using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using MacropadConfigurator.DTO;
using NLog;
using System.Net.Sockets;
using System.Text;

namespace MacropadConfigurator.Services;

public class CommunicationManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public event EventHandler? ConfigurationLoaded;

    private HidDevice? device = null;
    private HidStream? stream = null;
    
    private int reportLength = 0;
    private ReportDescriptor? reportDescriptor = null;
    private HidDeviceInputReceiver? inputReceiver = null;

    private readonly List<byte> receivedDataBuffer = [];
    private int expectedBytesToReceive = 0;

    public void FindMacropad()
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
            logger.Debug($"Found Macropad: {device.GetFriendlyName()}");
        else
            logger.Debug($"Device with VID=0x{Constants.VendorId:X4}, PID=0x{Constants.ProductId:X4} not found");
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
        }
    }
    private void InputReceiver_Received(object? sender, EventArgs e)
    {
        if (sender == null) return; // No sender, nothing to process

        var inputReceiver = (HidDeviceInputReceiver)sender;
        var packet = new byte[reportLength];

        // While there are reports in the queue, process them.
        while (inputReceiver.TryRead(packet, 0, out _))
        {
            // The first byte is the Report ID. This is not set by the HID-Project library and is always 0 for RawHID.
            if (packet[0] != Constants.RawHidInputReportId)
                continue; // Not the report we are looking for
            
            // The second byte is our actual command
            byte command = packet[1];

            logger.Info("Packet: ");
            var sb = new StringBuilder();
            for (int i = 0; i < packet.Length; i++)
            {
                sb.Append($"0x{packet[i]:X2} ");
            }
            logger.Info(sb.ToString());
        }
    }

    protected void OnConfigurationLoaded()
    {
        ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
    }
}
