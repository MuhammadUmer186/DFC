using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
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
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? OrderSource { get; set; }
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
        public string Message { get; set; }
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
    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }


}
