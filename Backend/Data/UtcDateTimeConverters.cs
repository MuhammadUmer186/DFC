using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RestaurantSystem.Data
{
    // SQL Server has no concept of DateTimeKind, so every DateTime EF Core reads back from the
    // database comes back as Kind=Unspecified — even though every timestamp this app writes is
    // DateTime.UtcNow. System.Text.Json then serializes Kind=Unspecified values with no "Z"/offset
    // suffix, so the browser's Date parser treats them as browser-local time instead of UTC,
    // silently shifting every order/report timestamp by the difference between the browser's zone
    // and UTC. Tagging every DateTime as Utc on the way out of the database (via
    // ApplicationDbContext.ConfigureConventions) fixes this for the whole schema at once.
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter() : base(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}
