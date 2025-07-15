using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;

namespace HIDCommunicationTest;

internal class Program
{
    // These must match the device's VID/PID.
    // The defaults for HID-Project's RawHID are:
    const int VendorId = 0x1B4F;
    const int ProductId = 0x9206;

    // The Report ID for RawHID input from the HID-Project library
    const int RawHidInputReportId = 0;

    // These must match the Arduino protocol definitions.
    const byte CMD_START_TRANSFER = (byte)'S';
    const byte CMD_DATA_PACKET = (byte)'D';
    const byte CMD_REQUEST_DATA = (byte)'R';
    const byte CMD_START_RESPONSE = (byte)'A';
    const byte CMD_RESPONSE_DATA_PACKET = (byte)'P';

    private static int reportLength;
    private static readonly List<byte> _receivedDataBuffer = new List<byte>();
    private static int _totalBytesToReceive = 0;

    private static readonly object _consoleLock = new object();

    static void Main(string[] args)
    {
        Console.WriteLine("Finding HID device...");

        // Find the device by VID and PID
        var device = DeviceList.Local.GetHidDevices(VendorId, ProductId).FirstOrDefault();

        if (device == null)
        {
            Console.WriteLine("Device not found. Make sure it's connected and the VID/PID are correct.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Device found! Attempting to open stream...");
        

        if (device.TryOpen(out var stream))
        {

        // HidSharp streams must be opened with a try/catch block
        reportLength = device.GetMaxOutputReportLength();
        var reportDescriptor = device.GetReportDescriptor();
        var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();

            using (stream)
            {
                Console.WriteLine("Stream opened successfully.");

                // Start a background thread to listen for incoming data
                inputReceiver.Received += InputReceiver_Received;
                inputReceiver.Stopped += (s, e) =>
                {
                    lock (_consoleLock)
                    {
                        Console.WriteLine("Input receiver stopped.");
                    }
                };
                inputReceiver.Start(stream);

                // Main menu loop
                while (true)
                {
                    Console.WriteLine("\n--- Main Menu ---");
                    Console.WriteLine("1. Send 400 bytes of data to Arduino");
                    Console.WriteLine("2. Request 400 bytes of data from Arduino");
                    Console.WriteLine("Q. Quit");
                    Console.Write("Choose an option: ");
                    var choice = Console.ReadKey().KeyChar;
                    Console.WriteLine();

                    if (choice == '1')
                    {
                        SendDataToArduino(stream);
                    }
                    else if (choice == '2')
                    {
                        RequestDataFromArduino(stream);
                    }
                    else if (char.ToUpper(choice) == 'Q')
                    {
                        break;
                    }
                    else
                    {
                        lock (_consoleLock) { Console.WriteLine("Invalid option."); }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Failed to open stream to the device.");
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

    private static void InputReceiver_Received(object sender, EventArgs e)
    {
        var inputReceiver = (HidDeviceInputReceiver)sender;
        var report = new byte[reportLength];

        // While there are reports in the queue, process them.
        while (inputReceiver.TryRead(report, 0, out _))
        {
            // The first byte is the Report ID. For RawHID, this is usually 3.
            // This is not set by the HID-Project library and is always 0 for RawHID.
            if (report[0] != RawHidInputReportId)
            {
                continue; // Not the report we are looking for
            }

            // The second byte is our actual command
            byte command = report[1];

            lock (_consoleLock)
            {
                if (command == CMD_START_RESPONSE)
                {
                    _receivedDataBuffer.Clear();
                    _totalBytesToReceive = report[2] | (report[3] << 8);

                    // The rest of the report is the first chunk of data
                    int dataLength = report.Length - 4;
                    _receivedDataBuffer.AddRange(report.Skip(4).Take(dataLength));

                    Console.WriteLine($"\n[Listener] Received START_RESPONSE. Expecting {_totalBytesToReceive} bytes.");
                }
                else if (command == CMD_RESPONSE_DATA_PACKET && _totalBytesToReceive > 0)
                {
                    int dataLength = report.Length - 2;
                    _receivedDataBuffer.AddRange(report.Skip(2).Take(dataLength));
                }

                // Check if the transfer is complete
                if (_totalBytesToReceive > 0 && _receivedDataBuffer.Count >= _totalBytesToReceive)
                {
                    Console.WriteLine("\n--- Arduino->PC Transfer Complete! ---");
                    Console.WriteLine($"Successfully received {_receivedDataBuffer.Count} bytes.");
                    Console.WriteLine("Printing first 20 bytes of the received message:");
                    for (int i = 0; i < 20; i++)
                    {
                        Console.Write($"0x{_receivedDataBuffer[i]:X2} ");
                    }
                    Console.WriteLine("\n--------------------------------------");

                    // Reset for the next transfer and re-draw the menu prompt
                    _totalBytesToReceive = 0;
                    _receivedDataBuffer.Clear();
                    Console.Write("\nChoose an option: ");
                }
            }
        }
    }

    // --- SENDER (PC -> Arduino) ---
    static void SendDataToArduino(HidStream stream)
    {
        byte[] dataToSend = new byte[400];
        for (int i = 0; i < dataToSend.Length; i++) { dataToSend[i] = (byte)(i % 256); }

        int payloadSize = reportLength - 1;
        int bytesSent = 0;

        Console.WriteLine($"\nSending {dataToSend.Length} bytes to the device.");

        // START Packet
        var startPacket = new byte[reportLength];
        startPacket[0] = 0x00;
        startPacket[1] = CMD_START_TRANSFER;
        startPacket[2] = (byte)(dataToSend.Length & 0xFF);
        startPacket[3] = (byte)((dataToSend.Length >> 8) & 0xFF);
        int firstChunkSize = payloadSize - 3;
        Array.Copy(dataToSend, 0, startPacket, 4, firstChunkSize);
        stream.Write(startPacket);
        bytesSent += firstChunkSize;
        Thread.Sleep(20);

        // DATA Packets
        while (bytesSent < dataToSend.Length)
        {
            var dataPacket = new byte[reportLength];
            dataPacket[0] = 0x00;
            dataPacket[1] = CMD_DATA_PACKET;
            int chunkSize = Math.Min(payloadSize - 1, dataToSend.Length - bytesSent);
            Array.Copy(dataToSend, bytesSent, dataPacket, 2, chunkSize);
            stream.Write(dataPacket);
            bytesSent += chunkSize;
            Thread.Sleep(20);
        }
        Console.WriteLine("Finished sending all data packets.");
    }

    // --- REQUESTER (PC -> Arduino) ---
    static void RequestDataFromArduino(HidStream stream)
    {
        var requestPacket = new byte[reportLength];

        requestPacket[0] = 0x00; // Report ID
        requestPacket[1] = CMD_REQUEST_DATA;

        Console.WriteLine("\nSending data request command to Arduino...");
        stream.Write(requestPacket);
    }
}
