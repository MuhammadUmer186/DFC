using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;

namespace RestaurantSystem.Services
{
    public class RestaurantClock : IRestaurantClock
    {
        private const string DefaultTimeZoneId = "Asia/Karachi";

        private readonly ApplicationDbContext _context;

        public RestaurantClock(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TimeZoneInfo> GetTimeZoneAsync()
        {
            var timeZoneId = await _context.SiteSettings
                .Where(s => s.Id == 1)
                .Select(s => s.TimeZoneId)
                .FirstOrDefaultAsync();

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
            }
        }

        public async Task<DateTime> GetLocalNowAsync()
        {
            var tz = await GetTimeZoneAsync();
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
    }
}
