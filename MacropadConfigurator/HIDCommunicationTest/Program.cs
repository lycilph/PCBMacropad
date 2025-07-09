using HidSharp;

namespace HIDCommunicationTest;

internal class Program
{
    // These must match the device's VID/PID.
    // The defaults for HID-Project's RawHID are:
    const int VendorId = 0x2341;
    const int ProductId = 0x8036;

    // These must match the Arduino protocol definitions.
    const byte CMD_START_TRANSFER = (byte)'S';
    const byte CMD_DATA_PACKET = (byte)'D';

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

        // HidSharp streams must be opened with a try/catch block
        var reportLength = device.GetMaxOutputReportLength();
        if (device.TryOpen(out var stream))
        {
            using (stream)
            {
                Console.WriteLine("Stream opened successfully.");

                // --- 1. Prepare the 400-byte message to send ---
                byte[] dataToSend = new byte[400];
                // Fill with sample data (e.g., 0, 1, 2, ..., 255, 0, 1...)
                for (int i = 0; i < dataToSend.Length; i++)
                {
                    dataToSend[i] = (byte)(i % 256);
                }

                // --- 2. Send the data using our protocol ---
                SendData(stream, reportLength, dataToSend);
            }
        }
        else
        {
            Console.WriteLine("Failed to open stream to the device.");
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

    static void SendData(HidStream stream, int reportLength, byte[] data)
    {
        // Get the report length from the device. For RawHID, this is usually 65
        // (1 byte for the Report ID, which is 0, plus 64 bytes of data).
        //int reportLength = stream.GetMaxOutputReportLength();
        int payloadSize = reportLength - 1; // 64

        Console.WriteLine($"\nSending {data.Length} bytes to the device.");
        Console.WriteLine($"Device report length: {reportLength} bytes.");

        int bytesSent = 0;

        // --- Send START Packet ---
        var startPacket = new byte[reportLength];
        startPacket[0] = 0x00; // Report ID must be 0 for RawHID
        startPacket[1] = CMD_START_TRANSFER;
        // Encode the total size into two bytes (little-endian)
        startPacket[2] = (byte)(data.Length & 0xFF);
        startPacket[3] = (byte)((data.Length >> 8) & 0xFF);

        int firstChunkSize = payloadSize - 3; // 64 - 3 = 61
        Array.Copy(data, 0, startPacket, 4, firstChunkSize);

        Console.WriteLine("Sending START packet...");
        stream.Write(startPacket);
        bytesSent += firstChunkSize;

        // A small delay is CRITICAL to give the Arduino time to process the last packet.
        Thread.Sleep(20);

        // --- Send DATA Packets ---
        while (bytesSent < data.Length)
        {
            var dataPacket = new byte[reportLength];
            dataPacket[0] = 0x00; // Report ID
            dataPacket[1] = CMD_DATA_PACKET;

            int chunkSize = Math.Min(payloadSize - 1, data.Length - bytesSent); // 63 or less
            Array.Copy(data, bytesSent, dataPacket, 2, chunkSize);

            Console.WriteLine($"Sending DATA packet... (Bytes {bytesSent} to {bytesSent + chunkSize - 1})");
            stream.Write(dataPacket);

            bytesSent += chunkSize;
            Thread.Sleep(20); // Delay between each packet
        }

        Console.WriteLine("\nFinished sending all data packets.");
    }
}
