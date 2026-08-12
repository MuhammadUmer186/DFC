
namespace RestaurantSystem.Helpers
{
    // "Business day" boundaries (dashboard/report "today", daily bucketing) shift at 3 AM
    // rather than midnight, and — critically — 3 AM in the restaurant's configured local time
    // zone (SiteSettings.TimeZoneId), not the hosting server's own OS clock/zone. All timestamps
    // in the database are UTC (see UtcDateTimeConverters.cs); callers get a TimeZoneInfo from
    // IRestaurantClock and pass it in here.
    public static class BusinessDayHelper
    {
        private const int ShiftHour = 3;

        // Which business day "right now" (in UTC) falls into, observed from the given time zone.
        public static DateOnly GetBusinessToday(TimeZoneInfo tz)
        {
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return GetBusinessDay(nowLocal);
        }

        // Which business day a stored UTC instant (e.g. an Order's CreatedAt) falls into.
        public static DateOnly GetBusinessToday(DateTime utcInstant, TimeZoneInfo tz)
        {
            var utc = utcInstant.Kind == DateTimeKind.Utc ? utcInstant : DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc);
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            return GetBusinessDay(local);
        }

        private static DateOnly GetBusinessDay(DateTime local)
        {
            return DateOnly.FromDateTime(
                local.Hour < ShiftHour ? local.AddDays(-1) : local
            );
        }

        // UTC instant at which this business day starts (03:00 local time).
        public static DateTime GetStart(DateOnly date, TimeZoneInfo tz)
        {
            var local = date.ToDateTime(new TimeOnly(ShiftHour, 0));
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), tz);
        }

        // UTC instant at which this business day ends (03:00 local time, next day) — exclusive.
        public static DateTime GetEnd(DateOnly date, TimeZoneInfo tz)
        {
            return GetStart(date.AddDays(1), tz);
        }
    }
}
