namespace RestaurantSystem.Services
{
    public interface ISystemMaintenanceService
    {
        /// Wipes every row from every table in the database, then reseeds the fixed baseline
        /// rows (default Area, ServiceTimeSettings, SiteSettings) that a freshly migrated
        /// database ships with, so the system is immediately usable afterward instead of
        /// crashing on missing singleton config rows.
        Task ClearAllDataAsync();
    }
}
