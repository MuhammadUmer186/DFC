using Microsoft.Extensions.Configuration;
using Printing.Helpers;
using RestaurantSystem.DTOs;
using RestaurantSystem.Printing;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

namespace Printing.Services
{
    public enum PrinterType
    {
        Usb1,
        Usb2,
        Ethernet,
        Bluetooth   // ✅ Added Bluetooth
    }

    public class PrintService
    {
        // All values come from the "Printing" section of appsettings.json so a new
        // machine can be pointed at its own Windows printer names / logo without a
        // rebuild. The fallbacks match the current shop-floor PC.
        private readonly string _usbPrinterName;   // customer copy  (Windows printer name — must match EXACTLY)
        private readonly string _usbPrinterName2;  // kitchen copy
        private readonly string _ethernetPrinterIp;
        private readonly int _ethernetPrinterPort;
        private readonly string _logoPath;

        // COM port for Bluetooth printer
        private readonly string _bluetoothComPort = "COM10";  // Adjust to your paired printer
        private readonly int _bluetoothBaudRate = 9600;

        public PrintService(IConfiguration config)
        {
            var p = config.GetSection("Printing");
            _usbPrinterName    = p["Usb1PrinterName"]  ?? "POS80Printer";
            _usbPrinterName2   = p["Usb2PrinterName"]  ?? "Black Copper 80";
            _ethernetPrinterIp = p["EthernetPrinterIp"] ?? "192.168.0.100";
            _ethernetPrinterPort = int.TryParse(p["EthernetPrinterPort"], out var port) ? port : 9100;
            _logoPath          = p["LogoPath"] ?? @"C:\Logo\Logo DFC.png";
        }

        public bool PrintReceipt(PrintReceiptDto dto, PrinterType type = PrinterType.Usb1)
        {
            string escPosText = EscPosReceiptBuilder.Build(dto);

            try
            {
                if (type == PrinterType.Usb1)
                {
                    byte[] logoBytes = EscPosImageHelper.GetLogoBytes(_logoPath);

                    byte[] textBytes = Encoding.UTF8.GetBytes(escPosText);

                    byte[] finalBytes = new byte[
                        logoBytes.Length + textBytes.Length
                    ];

                    Buffer.BlockCopy(
                        logoBytes,
                        0,
                        finalBytes,
                        0,
                        logoBytes.Length
                    );

                    Buffer.BlockCopy(
                        textBytes,
                        0,
                        finalBytes,
                        logoBytes.Length,
                        textBytes.Length
                    );

                    return RawPrinterHelper.SendBytesToPrinter(
                        _usbPrinterName,
                        finalBytes
                    );
                }
                else if (type == PrinterType.Usb2)
                {
                    byte[] logoBytes = EscPosImageHelper.GetLogoBytes(_logoPath);

                    byte[] textBytes = Encoding.UTF8.GetBytes(escPosText);

                    byte[] finalBytes = new byte[
                        logoBytes.Length + textBytes.Length
                    ];

                    Buffer.BlockCopy(
                        logoBytes,
                        0,
                        finalBytes,
                        0,
                        logoBytes.Length
                    );

                    Buffer.BlockCopy(
                        textBytes,
                        0,
                        finalBytes,
                        logoBytes.Length,
                        textBytes.Length
                    );

                    return RawPrinterHelper.SendBytesToPrinter(
                        _usbPrinterName2,
                        finalBytes
                    );
                }
                else if (type == PrinterType.Ethernet)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(escPosText);
                    using var client = new TcpClient(_ethernetPrinterIp, _ethernetPrinterPort);
                    using var stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    return true;
                }
                else if (type == PrinterType.Bluetooth)
                {
                    //using var serialPort = new SerialPort(_bluetoothComPort, _bluetoothBaudRate);
                    //serialPort.Open();
                    //serialPort.Write(escPosText);
                    //serialPort.Close();
                    return true;
                }
                else
                {
                    throw new ArgumentException("Unsupported printer type");
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine("Printing failed: " + ex.Message);
                return false;
            }
        }
    }
}
