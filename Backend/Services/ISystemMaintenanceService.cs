namespace RestaurantSystem.Services
{
    public interface ISystemMaintenanceService
    {
        /// Wipes every row from every table in the database except SiteSettings and
        /// ServiceTimeSettings (configured on the Settings page — left untouched so branding,
        /// currency, menu PDF and order-numbering config survive a reset), then reseeds the
        /// fixed baseline Area row that a freshly migrated database ships with.
        Task ClearAllDataAsync();
    }
}
