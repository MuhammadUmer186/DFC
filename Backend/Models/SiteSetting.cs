namespace RestaurantSystem.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }
        public string? HeroImageUrl { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? RestaurantName { get; set; }
        public string? LogoUrl { get; set; }

        // ✅ MENU PDF — the currently active menu PDF file. The QR code printed for customers
        // encodes a stable redirect endpoint (Public/menu-pdf), not this URL directly, so
        // re-uploading a new PDF (a new GUID filename) never invalidates already-printed QR codes.
        public string? MenuPdfUrl { get; set; }

        // ✅ ORDER SERIAL — customer-facing order numbers (Prefix + running number),
        // reset back to OrderSerialStartingNumber once per day at OrderSerialResetTime.
        public string OrderSerialPrefix { get; set; } = string.Empty;
        public int OrderSerialStartingNumber { get; set; } = 1;
        public TimeSpan OrderSerialResetTime { get; set; } = TimeSpan.Zero;

        // Rollover state: the last number issued, and which "business day" it belongs to
        // (a business day runs from OrderSerialResetTime to the next day's OrderSerialResetTime).
        public int OrderSerialCurrentNumber { get; set; }
        public DateTime? OrderSerialCurrentDate { get; set; }
    }
}
