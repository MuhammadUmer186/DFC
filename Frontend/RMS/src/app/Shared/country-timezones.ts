// Curated country -> primary IANA time zone + currency list, used by the Settings page's
// Country selector to auto-fill a sensible time zone and currency (admin can still override
// either afterward — e.g. a country spanning multiple zones, or a symbol preference).
export interface CountryTimeZone {
  country: string;
  timeZoneId: string;
  currencyCode: string;
  currencySymbol: string;
}

export const COUNTRY_TIMEZONES: CountryTimeZone[] = [
  { country: 'Pakistan', timeZoneId: 'Asia/Karachi', currencyCode: 'PKR', currencySymbol: 'Rs' },
  { country: 'India', timeZoneId: 'Asia/Kolkata', currencyCode: 'INR', currencySymbol: '₹' },
  { country: 'Bangladesh', timeZoneId: 'Asia/Dhaka', currencyCode: 'BDT', currencySymbol: '৳' },
  { country: 'Sri Lanka', timeZoneId: 'Asia/Colombo', currencyCode: 'LKR', currencySymbol: 'Rs' },
  { country: 'Nepal', timeZoneId: 'Asia/Kathmandu', currencyCode: 'NPR', currencySymbol: 'Rs' },
  { country: 'Afghanistan', timeZoneId: 'Asia/Kabul', currencyCode: 'AFN', currencySymbol: '؋' },
  { country: 'United Arab Emirates', timeZoneId: 'Asia/Dubai', currencyCode: 'AED', currencySymbol: 'AED' },
  { country: 'Saudi Arabia', timeZoneId: 'Asia/Riyadh', currencyCode: 'SAR', currencySymbol: 'SAR' },
  { country: 'Qatar', timeZoneId: 'Asia/Qatar', currencyCode: 'QAR', currencySymbol: 'QAR' },
  { country: 'Kuwait', timeZoneId: 'Asia/Kuwait', currencyCode: 'KWD', currencySymbol: 'KD' },
  { country: 'Bahrain', timeZoneId: 'Asia/Bahrain', currencyCode: 'BHD', currencySymbol: 'BD' },
  { country: 'Oman', timeZoneId: 'Asia/Muscat', currencyCode: 'OMR', currencySymbol: 'OMR' },
  { country: 'Turkey', timeZoneId: 'Europe/Istanbul', currencyCode: 'TRY', currencySymbol: '₺' },
  { country: 'Iran', timeZoneId: 'Asia/Tehran', currencyCode: 'IRR', currencySymbol: '﷼' },
  { country: 'Iraq', timeZoneId: 'Asia/Baghdad', currencyCode: 'IQD', currencySymbol: 'ID' },
  { country: 'Israel', timeZoneId: 'Asia/Jerusalem', currencyCode: 'ILS', currencySymbol: '₪' },
  { country: 'Jordan', timeZoneId: 'Asia/Amman', currencyCode: 'JOD', currencySymbol: 'JD' },
  { country: 'Egypt', timeZoneId: 'Africa/Cairo', currencyCode: 'EGP', currencySymbol: 'E£' },
  { country: 'China', timeZoneId: 'Asia/Shanghai', currencyCode: 'CNY', currencySymbol: '¥' },
  { country: 'Hong Kong', timeZoneId: 'Asia/Hong_Kong', currencyCode: 'HKD', currencySymbol: 'HK$' },
  { country: 'Japan', timeZoneId: 'Asia/Tokyo', currencyCode: 'JPY', currencySymbol: '¥' },
  { country: 'South Korea', timeZoneId: 'Asia/Seoul', currencyCode: 'KRW', currencySymbol: '₩' },
  { country: 'Singapore', timeZoneId: 'Asia/Singapore', currencyCode: 'SGD', currencySymbol: 'S$' },
  { country: 'Malaysia', timeZoneId: 'Asia/Kuala_Lumpur', currencyCode: 'MYR', currencySymbol: 'RM' },
  { country: 'Indonesia', timeZoneId: 'Asia/Jakarta', currencyCode: 'IDR', currencySymbol: 'Rp' },
  { country: 'Thailand', timeZoneId: 'Asia/Bangkok', currencyCode: 'THB', currencySymbol: '฿' },
  { country: 'Vietnam', timeZoneId: 'Asia/Ho_Chi_Minh', currencyCode: 'VND', currencySymbol: '₫' },
  { country: 'Philippines', timeZoneId: 'Asia/Manila', currencyCode: 'PHP', currencySymbol: '₱' },
  { country: 'Taiwan', timeZoneId: 'Asia/Taipei', currencyCode: 'TWD', currencySymbol: 'NT$' },
  { country: 'United Kingdom', timeZoneId: 'Europe/London', currencyCode: 'GBP', currencySymbol: '£' },
  { country: 'Ireland', timeZoneId: 'Europe/Dublin', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'France', timeZoneId: 'Europe/Paris', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Germany', timeZoneId: 'Europe/Berlin', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Spain', timeZoneId: 'Europe/Madrid', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Italy', timeZoneId: 'Europe/Rome', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Netherlands', timeZoneId: 'Europe/Amsterdam', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Belgium', timeZoneId: 'Europe/Brussels', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Switzerland', timeZoneId: 'Europe/Zurich', currencyCode: 'CHF', currencySymbol: 'CHF' },
  { country: 'Portugal', timeZoneId: 'Europe/Lisbon', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Sweden', timeZoneId: 'Europe/Stockholm', currencyCode: 'SEK', currencySymbol: 'kr' },
  { country: 'Norway', timeZoneId: 'Europe/Oslo', currencyCode: 'NOK', currencySymbol: 'kr' },
  { country: 'Denmark', timeZoneId: 'Europe/Copenhagen', currencyCode: 'DKK', currencySymbol: 'kr' },
  { country: 'Poland', timeZoneId: 'Europe/Warsaw', currencyCode: 'PLN', currencySymbol: 'zł' },
  { country: 'Greece', timeZoneId: 'Europe/Athens', currencyCode: 'EUR', currencySymbol: '€' },
  { country: 'Russia', timeZoneId: 'Europe/Moscow', currencyCode: 'RUB', currencySymbol: '₽' },
  { country: 'United States', timeZoneId: 'America/New_York', currencyCode: 'USD', currencySymbol: '$' },
  { country: 'Canada', timeZoneId: 'America/Toronto', currencyCode: 'CAD', currencySymbol: 'C$' },
  { country: 'Mexico', timeZoneId: 'America/Mexico_City', currencyCode: 'MXN', currencySymbol: 'MX$' },
  { country: 'Brazil', timeZoneId: 'America/Sao_Paulo', currencyCode: 'BRL', currencySymbol: 'R$' },
  { country: 'Argentina', timeZoneId: 'America/Argentina/Buenos_Aires', currencyCode: 'ARS', currencySymbol: 'AR$' },
  { country: 'Australia', timeZoneId: 'Australia/Sydney', currencyCode: 'AUD', currencySymbol: 'A$' },
  { country: 'New Zealand', timeZoneId: 'Pacific/Auckland', currencyCode: 'NZD', currencySymbol: 'NZ$' },
  { country: 'South Africa', timeZoneId: 'Africa/Johannesburg', currencyCode: 'ZAR', currencySymbol: 'R' },
  { country: 'Nigeria', timeZoneId: 'Africa/Lagos', currencyCode: 'NGN', currencySymbol: '₦' },
  { country: 'Kenya', timeZoneId: 'Africa/Nairobi', currencyCode: 'KES', currencySymbol: 'KSh' }
];
