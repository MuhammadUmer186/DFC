using ESCPOS_NET.Emitters;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Printing.Helpers
{
    public static class EscPosImageHelper
    {
        public static byte[] GetLogoBytes(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                // Logo not present on this machine — skip it rather than crashing the whole print job.
                return Array.Empty<byte>();
            }

            var printer = new EPSON();

            Bitmap logo = new Bitmap(imagePath);

            // Resize logo
            Bitmap resized = new Bitmap(logo, new Size(220, 220));

            // Full receipt width canvas
            Bitmap canvas = new Bitmap(576,220);

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(System.Drawing.Color.White);

                // Calculate center position
                int x = (576 - resized.Width) / 2;

                // Draw centered logo
                g.DrawImage(resized, x, 0);
            }

            // Convert centered canvas to byte[]
            using var ms = new MemoryStream();

            // IMPORTANT: Save canvas NOT resized
            canvas.Save(ms, ImageFormat.Png);

            // Center align command
            byte[] alignCenter = new byte[] { 0x1B, 0x61, 0x01 };

            // Image bytes
            byte[] imageBytes = printer.PrintImage(
                ms.ToArray(),
                true,
                true
            );

            // Left align after image
            byte[] alignLeft = new byte[] { 0x1B, 0x61, 0x00 };

            // Final combined bytes
            byte[] finalBytes = new byte[
                alignCenter.Length +
                imageBytes.Length +
                alignLeft.Length
            ];

            Buffer.BlockCopy(
                alignCenter,
                0,
                finalBytes,
                0,
                alignCenter.Length
            );

            Buffer.BlockCopy(
                imageBytes,
                0,
                finalBytes,
                alignCenter.Length,
                imageBytes.Length
            );

            Buffer.BlockCopy(
                alignLeft,
                0,
                finalBytes,
                alignCenter.Length + imageBytes.Length,
                alignLeft.Length
            );

            return finalBytes;
        }
    }
}