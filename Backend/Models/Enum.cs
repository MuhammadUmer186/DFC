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
}

