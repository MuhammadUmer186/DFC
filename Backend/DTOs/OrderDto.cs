using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Paid { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Discount { get; set; }

        // 🔥 Analytics fields
        public int? TakenByEmployeeId { get; set; }
        public string TakenByEmployeeName { get; set; }

        public int? CashierId { get; set; }
        public string? CashierName { get; set; }
        public string? CashierUserName { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? RejectReason { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? OrderSource { get; set; }
        public string? ServiceType { get; set; }
        public string? PaymentMethod { get; set; }
        public int? RiderId { get; set; }
        public string? RiderName { get; set; }
        public decimal? RiderCost { get; set; }
        public int? AreaId { get; set; }
        public string? AreaName { get; set; }
        public decimal? DeliveryFeeCharged { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderDealDto> Deals { get; set; }

        public int TotalQuantity =>
        (Items?.Sum(x => x.Quantity) ?? 0) +
        (Deals?.Sum(x => x.Quantity) ?? 0);
    }
    public class CreateOrderRequest
    {

        public List<CreateOrderItemRequest> Items { get; set; }
        public List<CreateDealOrderRequest>? Deals { get; set; } // NEW
        public bool? Paid { get; set; } = true;

    }
    public class PublicOrderRequest
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // "Cash" or "Online transfer"
        public string ServiceType { get; set; } = "Delivery"; // "DineIn" / "Takeaway" / "Delivery"
        public int? AreaId { get; set; } // required when ServiceType == "Delivery"; fee is looked up server-side

        public List<PublicOrderItemDto> Items { get; set; }
        public List<PublicOrderDealDto> Deals { get; set; }
    }
    public class PublicOrderItemDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
    public class PublicOrderDealDto
    {
        public int DealId { get; set; }
        public int Quantity { get; set; }
    }
    public class PublicOrderResponse
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string Message { get; set; }
    }
    public class ApproveOrderResult
    {
        public OrderDto Order { get; set; }
        public bool CounterPrinted { get; set; }
        public bool KitchenPrinted { get; set; }
    }
    public class PublicOrderStatusDto
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public string StatusLabel { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ServiceType { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderDealDto> Deals { get; set; }
    }
    public class PayOrderRequest
    {
        
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
    public class CancelOrderRequest
    {
        public int OrderId { get; set; }
        public int CashierId { get; set; }
    }
    public class AssignRiderDto
    {
        public int RiderId { get; set; }
    }
    public class UpdateDeliveryStatusRequest
    {
        public DeliveryStatus Status { get; set; }
    }
    public class RejectOrderRequest
    {
        public string? Reason { get; set; }
    }


    public class CreateOrderItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPriceOverride { get; set; }
    }
    public class OrderCountSummaryDto
    {
        public int TodayOrders { get; set; }
        public int WeeklyOrders { get; set; }
        public int MonthlyOrders { get; set; }

        public int TodayOnlineOrders { get; set; }
        public int TodaySiteOrders { get; set; }
        public int WeeklyOnlineOrders { get; set; }
        public int WeeklySiteOrders { get; set; }
        public int MonthlyOnlineOrders { get; set; }
        public int MonthlySiteOrders { get; set; }
    }
    public class TodayOnlineSummaryDto
    {
        public int OrderCount { get; set; }
        public decimal Sales { get; set; }
    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }


}
