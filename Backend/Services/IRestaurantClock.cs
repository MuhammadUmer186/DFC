namespace RestaurantSystem.Services
{
    // Resolves "now" in the restaurant's admin-configured time zone (Settings → Country, Time
    // Zone & Currency), instead of the hosting server's own OS clock/zone — the two are not the
    // same thing (this app's production server runs with a UTC system clock, for example).
    // Business-day boundaries (dashboard/report "today", the daily order-serial reset) must be
    // computed against the restaurant's local wall-clock time, not the server's.
    public interface IRestaurantClock
    {
        Task<TimeZoneInfo> GetTimeZoneAsync();
        Task<DateTime> GetLocalNowAsync();
    }
}
