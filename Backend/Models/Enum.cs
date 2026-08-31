using System.Text.Json.Serialization;

namespace RestaurantSystem.Models
{
    public enum OrderStatus
    {
        PendingApproval = 0,
        Queued = 1,
        Paid = 2,
        Cancelled = 3
    }
    public enum SalaryType
    {
        Daily = 1,
        Monthly = 2
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryStatus
    {
        Approved = 0,
        Preparing = 1,
        Enroute = 2,
        Delivered = 3,
        Rejected = 4
    }

    // ===== Offline-first / cloud-sync (Phase 1) =====
    // Role of a physical deployment in the branch/cloud topology. Stored as a
    // string in the DB (see ApplicationDbContext) so it stays human-readable when
    // rows are inspected across nodes.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NodeRole
    {
        Cloud = 0,
        Edge = 1
    }
}

