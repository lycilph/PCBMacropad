using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System.Runtime.InteropServices;
using System.Text;

namespace HidCommunicationTestV3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidSharp;

class Program
{
    // --- Arduino USB IDs ---
    private const int VendorId = 0x1B4F;   // PJRC default
    private const int ProductId = 0x9206;   // PJRC default

    private const int ConfigSize = 282; // sizeof(Layer) * 3

    static void Main()
    {
        var device = DeviceList.Local
                                .GetHidDevices(VendorId, ProductId)
                                .FirstOrDefault();

        if (device == null)
        {
            Console.WriteLine("❌ Arduino RawHID device not found.");
            return;
        }

        if (!device.TryOpen(out HidStream stream))
        {
            Console.WriteLine("❌ Failed to open HID stream.");
            return;
        }

        stream.ReadTimeout = 3000;
        stream.WriteTimeout = 3000;

        int inLen = device.GetMaxInputReportLength();   // Usually 65 on Windows
        int outLen = device.GetMaxOutputReportLength(); // Usually 65 on Windows

        Console.WriteLine($"✔ Connected to {device.ProductName} (IN={inLen}, OUT={outLen})");

        while (true)
        {
            Console.WriteLine("[G]et config  [S]et dummy config  [Q]uit");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Q) break;

            switch (key)
            {
                case ConsoleKey.G:
                    GetConfig(stream, inLen, outLen);
                    break;
                case ConsoleKey.S:
                    SetConfig(stream, inLen, outLen);
                    break;
            }
        }
    }

    // --- Send 'G' and read config back from Arduino ---
    private static void GetConfig(HidStream stream, int inLen, int outLen)
    {
        byte[] outBuf = new byte[outLen];
        int offsetOut = (outLen > 64) ? 1 : 0;
        outBuf[offsetOut] = (byte)'G'; // Send 'G' command
        stream.Write(outBuf);
        Console.WriteLine("→ Sent 'G'");

        List<byte> data = new();

        while (data.Count < ConfigSize)
        {
            byte[] inBuf = new byte[inLen];
            int read = stream.Read(inBuf, 0, inLen);
            Console.WriteLine($"DEBUG: Read {read} bytes from device");

            if (read != inLen)
            {
                Console.WriteLine($"⚠ Expected {inLen} bytes but got {read}. Aborting.");
                return;
            }

            int offsetIn = (inLen > 64) ? 1 : 0;

            // First packet includes the 'C' command byte
            if (data.Count == 0)
            {
                if (inBuf[offsetIn] != (byte)'C')
                {
                    Console.WriteLine($"⚠ Unexpected first byte (expected 'C'): 0x{inBuf[offsetIn]:X2}");
                    return;
                }

                offsetIn += 1; // Skip 'C' byte
            }

            int usableBytes = inLen - offsetIn;
            if (usableBytes <= 0 || offsetIn >= inBuf.Length)
            {
                Console.WriteLine("⚠ Invalid offset or no usable bytes. Aborting.");
                return;
            }

            data.AddRange(inBuf.Skip(offsetIn).Take(usableBytes));
            Console.WriteLine($"← Packet received ({data.Count}/{ConfigSize} bytes)");
        }

        Console.WriteLine("✔ All config data received.");
        ParseConfig(data.ToArray());
    }

    // --- Send 'S' and write dummy config to Arduino ---
    private static void SetConfig(HidStream stream, int inLen, int outLen)
    {
        byte[] config = GenerateDummyConfig();
        byte[] outBuf = new byte[outLen];
        int offsetOut = (outLen > 64) ? 1 : 0;

        outBuf[offsetOut] = (byte)'S';
        stream.Write(outBuf);
        System.Threading.Thread.Sleep(5); // small delay before streaming

        Console.WriteLine("→ Sent 'S', sending config...");

        int index = 0;
        while (index < config.Length)
        {
            int chunkSize = Math.Min(64, config.Length - index);
            byte[] chunk = new byte[outLen];
            Buffer.BlockCopy(config, index, chunk, offsetOut, chunkSize);
            stream.Write(chunk);
            index += chunkSize;
            Console.WriteLine($"→ Sent chunk ({index}/{config.Length})");
        }

        Console.WriteLine("→ All config bytes sent. Waiting for ACK...");

        byte[] inBuf = new byte[inLen];
        int read = stream.Read(inBuf, 0, inLen);
        if (read > 0)
        {
            int offsetIn = (inLen > 64) ? 1 : 0;
            byte code = inBuf[offsetIn];
            if (code == (byte)'A')
                Console.WriteLine("✔ ACK from Arduino");
            else if (code == (byte)'E')
                Console.WriteLine("❌ ERROR from Arduino");
            else
                Console.WriteLine($"❓ Unknown response: 0x{code:X2}");
        }
        else
        {
            Console.WriteLine("⚠ No response received.");
        }
    }

    // --- Dummy config with recognizable change ---
    private static byte[] GenerateDummyConfig()
    {
        byte[] buffer = new byte[ConfigSize];
        Array.Fill<byte>(buffer, 0);

        string dummyName = "PC_Config";
        byte[] nameBytes = Encoding.ASCII.GetBytes(dummyName);
        int offset = 188; // Layer 2 name field: 94 bytes per layer, +12

        Array.Copy(nameBytes, 0, buffer, offset, Math.Min(12, nameBytes.Length));
        return buffer;
    }

    // --- Minimal config reader: prints layer names and enable flags ---
    private static void ParseConfig(byte[] data)
    {
        for (int i = 0; i < 3; i++)
        {
            int baseIdx = i * 94;
            string name = Encoding.ASCII.GetString(data, baseIdx, 12).TrimEnd('\0');
            bool isEnabled = data[baseIdx + 12] != 0;
            Console.WriteLine($"Layer {i}: \"{name}\"  Enabled={isEnabled}");
        }
    }
}