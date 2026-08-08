using Microsoft.AspNetCore.SignalR;
using RestaurantSystem.Models;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Hubs
{
    public class OrderHub : Hub
    {
        // Called by client to join the "Queue" group
        public async Task JoinQueue()
        {
            // In OrderHub
            await Groups.AddToGroupAsync(Context.ConnectionId, "OrderQueue");

        }

        // Send new order
        public async Task SendQueuedOrder(Order order)
        {
            var dto = new OrderQueueDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt.ToString("s"),
                Paid = order.Paid,
                TakenByEmployeeName = order.TakenByEmployee?.Name,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    MenuItemName = oi.MenuItem.Name,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList(),
                Deals = order.OrderDeals.Select(od => new OrderDealDto
                {
                    DealName = od.Deal.DealName,
                    DealPrice = od.DealPrice,
                    Quantity = od.Quantity,
                    Items = od.Deal.DealItems.Select(di => new DealMenuItemDto
                    {
                        Name = di.MenuItem.Name,
                        Price = di.MenuItem.Price,
                        Quantity = di.Quantity
                    }).ToList()
                }).ToList()
            };

            await Clients.Group("Queue").SendAsync("OrderQueued", dto);
        }

        public async Task SendOrderPaid(int orderId)
        {
            await Clients.Group("Queue").SendAsync("OrderPaid", orderId);
        }

        public async Task SendOrderCancelled(int orderId)
        {
            await Clients.Group("Queue").SendAsync("OrderCancelled", orderId);
        }
    }
    public class OrderQueueDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedAt { get; set; }
        public bool Paid { get; set; }
        public string TakenByEmployeeName { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderDealDto> Deals { get; set; } = new();
    }

    

    

   

}
