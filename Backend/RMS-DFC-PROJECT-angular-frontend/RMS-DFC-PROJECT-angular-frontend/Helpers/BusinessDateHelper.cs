
namespace RestaurantSystem.Helpers
    {
        public static class BusinessDayHelper
        {
            // Business day starts at 3 AM
            private const int ShiftHour = 3;

            // Returns the business "today" date
            public static DateOnly GetBusinessToday()
            {
                var now = DateTime.Now;
                return DateOnly.FromDateTime(
                    now.Hour < ShiftHour ? now.AddDays(-1) : now
                );
            }
            public static DateOnly GetBusinessToday(DateTime dt)
            {
                return DateOnly.FromDateTime(
                    dt.Hour < ShiftHour ? dt.AddDays(-1) : dt
                );
            }

            // Start of business day (03:00 AM)
            public static DateTime GetStart(DateOnly date)
            {
                return date.ToDateTime(new TimeOnly(ShiftHour, 0));
            }

            // End of business day (03:00 AM next day) – exclusive
            public static DateTime GetEnd(DateOnly date)
            {
                return date.AddDays(1).ToDateTime(new TimeOnly(ShiftHour, 0));
            }
        }
    }