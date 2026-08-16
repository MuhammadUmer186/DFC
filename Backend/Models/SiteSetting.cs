namespace RestaurantSystem.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }
        public string? HeroImageUrl { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? RestaurantName { get; set; }
        public string? LogoUrl { get; set; }

        // ✅ COMPANY BRANDING — the legal/parent company name & logo, distinct from the
        // restaurant brand shown on the customer site and RMS sidebar. Used on the RMS
        // navbar and login page.
        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }

        // ✅ COUNTRY & TIME ZONE — which country the restaurant operates in and the IANA time
        // zone (e.g. "Asia/Karachi") used to display "system time" (dashboard clock, etc.)
        // regardless of where the browser viewing it physically is.
        public string Country { get; set; } = "Pakistan";
        public string TimeZoneId { get; set; } = "Asia/Karachi";
        public string CurrencyCode { get; set; } = "PKR";
        public string CurrencySymbol { get; set; } = "Rs";

        // ✅ MENU PDF — the currently active menu PDF file. The QR code printed for customers
        // encodes a stable redirect endpoint (Public/menu-pdf), not this URL directly, so
        // re-uploading a new PDF (a new GUID filename) never invalidates already-printed QR codes.
        public string? MenuPdfUrl { get; set; }

        // ✅ GOOGLE MAPS LOCATION — the restaurant's Google Maps link (e.g. copied from Maps'
        // "Share" button). Same indirection as MenuPdfUrl: the printed QR encodes the stable
        // Public/location redirect, not this link directly, so updating the address later
        // (branch move, corrected pin) never invalidates an already-printed QR code.
        public string? GoogleMapsUrl { get; set; }

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
