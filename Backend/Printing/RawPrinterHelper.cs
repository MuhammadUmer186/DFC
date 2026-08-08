using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Printing.Helpers
{
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName = "";
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile = "";
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType = "";
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int Level, [In] DOCINFOA pDocInfo);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string data)
        {
            IntPtr pBytes;
            int dwCount = Encoding.UTF8.GetByteCount(data);

            pBytes = Marshal.StringToCoTaskMemAnsi(data);
            bool success = false;

            if (OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                var docInfo = new DOCINFOA { pDocName = "POS Receipt", pDataType = "RAW" };
                if (StartDocPrinter(hPrinter, 1, docInfo))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        success = WritePrinter(hPrinter, pBytes, dwCount, out int written);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            Marshal.FreeCoTaskMem(pBytes);
            return success;
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr pUnmanagedBytes;
            int dwCount = bytes.Length;

            pUnmanagedBytes = Marshal.AllocCoTaskMem(dwCount);

            Marshal.Copy(bytes, 0, pUnmanagedBytes, dwCount);

            bool success = false;

            if (OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                var docInfo = new DOCINFOA
                {
                    pDocName = "POS Receipt",
                    pDataType = "RAW"
                };

                if (StartDocPrinter(hPrinter, 1, docInfo))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        success = WritePrinter(
                            hPrinter,
                            pUnmanagedBytes,
                            dwCount,
                            out int written
                        );

                        EndPagePrinter(hPrinter);
                    }

                    EndDocPrinter(hPrinter);
                }

                ClosePrinter(hPrinter);
            }

            Marshal.FreeCoTaskMem(pUnmanagedBytes);

            return success;
        }
    }
}
