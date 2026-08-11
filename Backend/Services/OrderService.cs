using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAI.Graders;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using RestaurantSystem.Hubs;
using RestaurantSystem.Models;
using System.Security.Claims;
using PrintSvc = Printing.Services.PrintService;
using PrinterKind = Printing.Services.PrinterType;

namespace RestaurantSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IKitchenOutService _kitchenOutService;
        private readonly PrintSvc _printService;
        public OrderService(ApplicationDbContext context, IHubContext<OrderHub> hub, IKitchenOutService kitchenOutService, PrintSvc printService)
        {
            _context = context;
            _hub = hub;
            _kitchenOutService = kitchenOutService;
            _printService = printService;
        }

        // Shared by CreateAsync and ApproveOnlineOrderAsync — expands items/deals into raw-item quantities via recipes
        private async Task<Dictionary<int, decimal>> BuildRawItemConsumptionAsync(
            IEnumerable<OrderItem> orderItems, IEnumerable<OrderDeal> orderDeals)
        {
            var menuItemConsumption = new Dictionary<int, decimal>();

            foreach (var oi in orderItems)
            {
                if (!menuItemConsumption.ContainsKey(oi.MenuItemId))
                    menuItemConsumption[oi.MenuItemId] = 0;

                menuItemConsumption[oi.MenuItemId] += oi.Quantity;
            }

            foreach (var od in orderDeals)
            {
                var dealItems = await _context.DealItems
                    .Where(x => x.DealId == od.DealId)
                    .ToListAsync();

                foreach (var di in dealItems)
                {
                    if (!menuItemConsumption.ContainsKey(di.MenuItemId))
                        menuItemConsumption[di.MenuItemId] = 0;

                    menuItemConsumption[di.MenuItemId] += di.Quantity * od.Quantity;
                }
            }

            var rawItemConsumption = new Dictionary<int, decimal>();

            foreach (var entry in menuItemConsumption)
            {
                var recipes = await _context.MenuRecipes
                    .Where(r => r.MenuItemId == entry.Key)
                    .ToListAsync();

                foreach (var recipe in recipes)
                {
                    if (!rawItemConsumption.ContainsKey(recipe.RawItemId))
                        rawItemConsumption[recipe.RawItemId] = 0;

                    rawItemConsumption[recipe.RawItemId] += recipe.QuantityRequired * entry.Value;
                }
            }

            return rawItemConsumption;
        }

        // Builds the item+deal line list for a delivery slip (Approve print + manual reprint)
        private List<PrintItemDto> BuildDeliveryReceiptItems(Order order)
        {
            var items = new List<PrintItemDto>();

            items.AddRange(order.OrderItems.Select(oi => new PrintItemDto
            {
                Name = oi.MenuItem?.Name ?? "Item",
                Quantity = oi.Quantity,
                Price = oi.UnitPrice
            }));

            items.AddRange(order.OrderDeals.Select(od => new PrintItemDto
            {
                Name = od.Deal?.DealName ?? "Deal",
                Quantity = od.Quantity,
                Price = od.DealPrice
            }));

            return items;
        }

        // Maps an order's ServiceType to the banner label printed on its slip
        private static string ServiceTypeSlipLabel(string? serviceType) => serviceType switch
        {
            "DineIn" => "DINE-IN ORDER",
            "Takeaway" => "TAKEAWAY ORDER",
            _ => "DELIVERY ORDER"
        };

        // Prints both delivery-slip copies (counter=usb1, kitchen=usb2). Failures are caught per-copy —
        // a printer being offline should not block the approval itself.
        private (bool CounterPrinted, bool KitchenPrinted) PrintDeliverySlips(Order order)
        {
            var items = BuildDeliveryReceiptItems(order);

            PrintReceiptDto BuildDto(string copyType) => new PrintReceiptDto
            {
                CopyType = copyType,
                OrderNo = order.Id,
                Total = order.TotalAmount,
                Discount = 0,
                FinalTotal = order.TotalAmount,
                Items = items,
                OrderTypeLabel = ServiceTypeSlipLabel(order.ServiceType),
                CustomerName = order.CustomerName,
                CustomerPhone = order.PhoneNumber,
                CustomerAddress = order.Address
            };

            bool counterOk = false, kitchenOk = false;

            try { counterOk = _printService.PrintReceipt(BuildDto("customer"), PrinterKind.Usb1); }
            catch { counterOk = false; }

            try { kitchenOk = _printService.PrintReceipt(BuildDto("kitchen"), PrinterKind.Usb2); }
            catch { kitchenOk = false; }

            return (counterOk, kitchenOk);
        }

        // CREATE ORDER
        public async Task<OrderDto> CreateAsync(
    CreateOrderRequest request,
    decimal discount,
    ClaimsPrincipal userClaims,
            bool skipStockCheck = false)
        {
            // ================= BASIC VALIDATION =================
            if ((request.Items == null || !request.Items.Any()) &&
                (request.Deals == null || !request.Deals.Any()))
                throw new Exception("Order must contain at least one item or deal");

            if (discount < 0 || discount > 100)
                throw new Exception("Invalid discount value");

            // ================= USER / ROLE =================
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            var employeeIdClaim = userClaims.FindFirst("EmployeeId")?.Value;

            var allowedRoles = new[] { "Waiter", "Cashier", "Admin", "MainAdmin", "SuperAdmin" };

            if (!allowedRoles.Contains(userRole))
                throw new Exception("Invalid or unauthorized user");
            int? takenByEmployeeId = null;

            // Admin does not need employee id
            if (userRole != "Admin" && userRole != "MainAdmin" && userRole != "SuperAdmin")
            {
                if (string.IsNullOrEmpty(employeeIdClaim))
                    throw new Exception("EmployeeId missing in token");

                takenByEmployeeId = int.Parse(employeeIdClaim);
            }



            // ================= EMPLOYEE =================


            // ================= TRANSACTION =================
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                bool isPaid = request.Paid ?? true;
                var userName = userClaims.FindFirst(ClaimTypes.Name)?.Value;

                var order = new Order
                {
                    CreatedAt = DateTime.Now,
                    Paid = isPaid,
                    PaidAt = isPaid ? DateTime.Now : null,
                    Status = isPaid ? OrderStatus.Paid : OrderStatus.Queued,
                    TakenByEmployeeId = isPaid ? takenByEmployeeId : null,
                    CashierId = isPaid ? takenByEmployeeId : null,
                    CashierUserName = isPaid ? userName : null,
                    PaymentMethod = isPaid ? "Cash" : null,

                    OrderItems = new List<OrderItem>(),
                    OrderDeals = new List<OrderDeal>()
                };


                decimal itemsTotal = 0;
                decimal dealsTotal = 0;

                // ================= NORMAL ITEMS =================
                if (request.Items != null && request.Items.Any())
                {
                    var itemIds = request.Items.Select(i => i.MenuItemId).ToList();

                    var menuItems = await _context.MenuItems
                        .Where(m => itemIds.Contains(m.Id))
                        .ToListAsync();

                    foreach (var item in request.Items)
                    {
                        var menu = menuItems.FirstOrDefault(m => m.Id == item.MenuItemId);
                        if (menu == null)
                            throw new Exception($"Menu item not found: {item.MenuItemId}");

                        decimal unitPrice =
                        item.UnitPriceOverride.HasValue
                            ? item.UnitPriceOverride.Value
                            : menu.Price;

                        if (unitPrice < 0)
                            throw new Exception("Unit price cannot be negative");

                        var orderItem = new OrderItem
                        {
                            MenuItemId = menu.Id,
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice
                        };

                        itemsTotal += orderItem.UnitPrice * orderItem.Quantity;
                        order.OrderItems.Add(orderItem);

                    }
                }

                // ================= DEALS =================
                if (request.Deals != null && request.Deals.Any())
                {
                    var dealIds = request.Deals.Select(d => d.DealId).ToList();

                    var deals = await _context.Deals
                        .Where(d => dealIds.Contains(d.Id))
                        .ToListAsync();

                    foreach (var d in request.Deals)
                    {
                        var deal = deals.FirstOrDefault(x => x.Id == d.DealId);
                        if (deal == null)
                            throw new Exception($"Deal not found: {d.DealId}");

                        var orderDeal = new OrderDeal
                        {
                            DealId = deal.Id,
                            Quantity = d.Quantity,
                            DealPrice = deal.FinalPrice
                        };

                        dealsTotal += deal.FinalPrice * d.Quantity;
                        order.OrderDeals.Add(orderDeal);
                    }
                }

                // ================= DISCOUNT (ITEMS ONLY) =================
                decimal discountAmount = 0;
                decimal finalItemsTotal = itemsTotal;

                if (discount > 0 && itemsTotal > 0)
                {
                    discountAmount = (itemsTotal * discount) / 100;
                    finalItemsTotal -= discountAmount;
                }

                order.TotalAmount = finalItemsTotal + dealsTotal;
                

               if (order.TotalAmount <= 0)
                    throw new Exception("Order total must be greater than zero");

                // ================= MENU RECIPE → KITCHEN STOCK =================
                if (!skipStockCheck)
                {
                    var rawItemConsumption = await BuildRawItemConsumptionAsync(order.OrderItems, order.OrderDeals);

                    await _kitchenOutService.ConsumeAsync(
                        rawItemConsumption,
                        DateTime.Now,
                        takenByEmployeeId
                    );
                }


                // ================= SAVE =================
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                var dto = await GetByIdAsync(order.Id);
                dto.Discount = discountAmount;// returns OrderDto
                if (!order.Paid)
                {
                    await _hub.Clients
                        .Group("OrderQueue")
                        .SendAsync("OrderQueued", dto);
                }

                // Fired for every order regardless of paid status — lets things like the
                // sidebar's Today's Summary auto-refresh instead of needing a hard refresh.
                await _hub.Clients
                    .Group("OrderQueue")
                    .SendAsync("OrderCreated", dto);

                await transaction.CommitAsync();

                return await GetByIdAsync(order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> CreateOnlineOrderAsync(PublicOrderRequest request)
        {
            // NOTE: CreateAsync() below opens + commits its own DB transaction internally, so this
            // method can't wrap the whole thing in a second outer transaction (EF Core doesn't support
            // nesting BeginTransactionAsync on the same context). The online-field update just below
            // is therefore a separate, single-statement SaveChangesAsync immediately after — same as
            // the pre-existing behavior here, just no longer silently commented out.

            // 🟢 Call your existing method BUT:
            // - No discount
            // - Fake system user (Admin bypass)

            var fakeClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            }));

            var createOrderRequest = new CreateOrderRequest
            {
                Items = request.Items?.Select(x => new CreateOrderItemRequest
                {
                    MenuItemId = x.MenuItemId,
                    Quantity = x.Quantity
                }).ToList(),

                Deals = request.Deals?.Select(x => new CreateDealOrderRequest
                {
                    DealId = x.DealId,
                    Quantity = x.Quantity
                }).ToList(),

                Paid = false // 🔥 ONLINE = NOT PAID
            };

            var orderDto = await CreateAsync(createOrderRequest, 0, fakeClaims, true);

            // 🔴 GET ACTUAL ENTITY (IMPORTANT)
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderDto.Id);

            // 🟢 SET ONLINE FIELDS
            order.CustomerName = request.CustomerName;
            order.PhoneNumber = request.PhoneNumber;
            order.Address = request.Address;
            order.Latitude = request.Latitude;
            order.Longitude = request.Longitude;
            order.PaymentMethod = request.PaymentMethod;
            order.ServiceType = request.ServiceType;

            order.OrderSource = "Online";
            order.Status = OrderStatus.PendingApproval; // sits here until staff approves — not kitchen-visible yet

            // Delivery fee is looked up server-side from the selected Area (never trust a client-supplied
            // amount) and snapshotted onto the order so it stays accurate even if the Area's fee changes later.
            if (request.ServiceType == "Delivery")
            {
                if (request.AreaId == null)
                    throw new Exception("An area must be selected for delivery orders");

                var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == request.AreaId && a.IsActive);
                if (area == null)
                    throw new Exception("Selected area is not available");

                order.AreaId = area.Id;
                order.DeliveryFeeCharged = area.DeliveryFee;
                order.TotalAmount += area.DeliveryFee;
            }

            await _context.SaveChangesAsync();

            var dto = await GetByIdAsync(order.Id);

            // 🔔 Notify RMS (REAL-TIME)
            await _hub.Clients
                .Group("OrderQueue")
                .SendAsync("NewOnlineOrder", dto);

            return dto;
        }

        public async Task<PublicOrderStatusDto> GetPublicOrderStatusAsync(int orderId, string phone)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.OrderSource == "Online");

            if (order == null) return null;

            // Compare last 10 digits so "+92 300..." / "0300..." style differences don't block a genuine match
            string Digits(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());
            var given = Digits(phone);
            var onFile = Digits(order.PhoneNumber);
            var last = (string s) => s.Length >= 10 ? s[^10..] : s;

            if (given.Length < 7 || last(given) != last(onFile))
                return null;

            string statusLabel;
            if (order.Status == OrderStatus.PendingApproval)
            {
                statusLabel = "Order received — awaiting confirmation";
            }
            else if (order.DeliveryStatus != null)
            {
                statusLabel = order.DeliveryStatus switch
                {
                    RestaurantSystem.Models.DeliveryStatus.Approved => "Confirmed — order received by kitchen",
                    RestaurantSystem.Models.DeliveryStatus.Preparing => "Preparing your order",
                    RestaurantSystem.Models.DeliveryStatus.Enroute => "Out for delivery",
                    RestaurantSystem.Models.DeliveryStatus.Delivered => "Delivered — enjoy your meal!",
                    RestaurantSystem.Models.DeliveryStatus.Rejected => "Sorry, item not available — order rejected",
                    _ => order.DeliveryStatus.ToString()
                };
            }
            else
            {
                statusLabel = order.Status switch
                {
                    OrderStatus.Paid => "Completed",
                    OrderStatus.Cancelled => "Cancelled",
                    _ => order.Status.ToString()
                };
            }

            return new PublicOrderStatusDto
            {
                Id = order.Id,
                StatusLabel = statusLabel,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                ServiceType = order.ServiceType,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem?.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),
                Deals = order.OrderDeals.Select(d => new OrderDealDto
                {
                    DealId = d.DealId,
                    DealName = d.Deal.DealName,
                    DealPrice = d.DealPrice,
                    Quantity = d.Quantity
                }).ToList()
            };
        }

        public async Task<List<OrderDto>> GetPendingOnlineOrdersAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.PendingApproval && o.OrderSource == "Online")
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal).ThenInclude(d => d.DealItems).ThenInclude(di => di.MenuItem)
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .OrderBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return orders.Select(ToOnlineOrderDto).ToList();
        }

        // Online orders that have been approved but not yet delivered — feeds the admin
        // "In Progress" view. FIFO: oldest first, matching how staff should naturally work them.
        public async Task<List<OrderDto>> GetActiveOnlineOrdersAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Queued && o.OrderSource == "Online"
                    && o.DeliveryStatus != RestaurantSystem.Models.DeliveryStatus.Delivered)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal).ThenInclude(d => d.DealItems).ThenInclude(di => di.MenuItem)
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .OrderBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return orders.Select(ToOnlineOrderDto).ToList();
        }

        private static OrderDto ToOnlineOrderDto(Order order) => new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Paid = order.Paid,
            Status = order.Status,
            DeliveryStatus = order.DeliveryStatus,
            CustomerName = order.CustomerName,
            PhoneNumber = order.PhoneNumber,
            Address = order.Address,
            Latitude = order.Latitude,
            Longitude = order.Longitude,
            OrderSource = order.OrderSource,
            ServiceType = order.ServiceType,
            PaymentMethod = order.PaymentMethod,
            RiderId = order.RiderId,
            RiderName = order.Rider?.Name,
            RiderCost = order.RiderCost,
            AreaId = order.AreaId,
            AreaName = order.Area?.Name,
            DeliveryFeeCharged = order.DeliveryFeeCharged,
            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItem?.Name,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList(),
            Deals = order.OrderDeals.Select(d => new OrderDealDto
            {
                DealId = d.DealId,
                DealName = d.Deal.DealName,
                DealPrice = d.DealPrice,
                Quantity = d.Quantity,
                Items = d.Deal.DealItems.Select(x => new DealMenuItemDto
                {
                    MenuItemId = x.MenuItemId,
                    Name = x.MenuItem.Name,
                    Price = x.MenuItem.Price,
                    Quantity = x.Quantity
                }).ToList()
            }).ToList()
        };

        public async Task<ApproveOrderResult> ApproveOnlineOrderAsync(int orderId, ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Cashier" && role != "Admin" && role != "MainAdmin" && role != "SuperAdmin")
                throw new Exception("Only cashier or admin can approve orders");

            using var transaction = await _context.Database.BeginTransactionAsync();

            Order order;
            try
            {
                order = await _context.Orders
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                    .Include(o => o.OrderDeals).ThenInclude(od => od.Deal).ThenInclude(d => d.DealItems).ThenInclude(di => di.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new Exception("Order not found");

                if (order.Status != OrderStatus.PendingApproval)
                    throw new Exception("Order is not pending approval");

                var rawItemConsumption = await BuildRawItemConsumptionAsync(order.OrderItems, order.OrderDeals);
                await _kitchenOutService.ConsumeAsync(rawItemConsumption, DateTime.Now, null);

                order.Status = OrderStatus.Queued;
                order.DeliveryStatus = RestaurantSystem.Models.DeliveryStatus.Approved;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            var dto = await GetByIdAsync(order.Id);

            await _hub.Clients.Group("OrderQueue").SendAsync("OrderQueued", dto);
            await _hub.Clients.Group("OrderQueue").SendAsync("OnlineOrderApproved", order.Id);

            var (counterOk, kitchenOk) = PrintDeliverySlips(order);

            return new ApproveOrderResult { Order = dto, CounterPrinted = counterOk, KitchenPrinted = kitchenOk };
        }

        public async Task<OrderDto> RejectOnlineOrderAsync(int orderId, string? reason, ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            var employeeIdClaim = userClaims.FindFirst("EmployeeId")?.Value;

            if (role != "Cashier" && role != "Admin" && role != "MainAdmin" && role != "SuperAdmin")
                throw new Exception("Only cashier or admin can reject orders");

            bool isAdmin = role == "Admin" || role == "MainAdmin" || role == "SuperAdmin";

            if (!isAdmin && string.IsNullOrEmpty(employeeIdClaim))
                throw new Exception("EmployeeId missing in token");

            int? rejectedByEmployeeId = isAdmin ? null : int.Parse(employeeIdClaim);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.PendingApproval)
                throw new Exception("Order is not pending approval");

            order.Status = OrderStatus.Cancelled;
            order.DeliveryStatus = RestaurantSystem.Models.DeliveryStatus.Rejected;
            order.CancelledAt = DateTime.Now;
            order.CancelledByEmployeeId = rejectedByEmployeeId;
            order.CancelledByUserName = userClaims.FindFirst(ClaimTypes.Name)?.Value;
            order.RejectReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            await _context.SaveChangesAsync();

            await _hub.Clients.Group("OrderQueue").SendAsync("OnlineOrderRejected", order.Id);

            return await GetByIdAsync(order.Id);
        }

        // Fulfillment must move one step at a time — Approved → Preparing → (Enroute →) Delivered.
        // Delivery orders pass through Enroute (rider is on the way); pickup/dine-in orders skip it
        // since there's no rider leg. Returns null when there is no legal next step.
        private static DeliveryStatus? GetNextDeliveryStatus(DeliveryStatus current, string? serviceType)
        {
            bool isDelivery = (serviceType ?? "Delivery") == "Delivery";
            return current switch
            {
                DeliveryStatus.Approved => DeliveryStatus.Preparing,
                DeliveryStatus.Preparing => isDelivery ? DeliveryStatus.Enroute : DeliveryStatus.Delivered,
                DeliveryStatus.Enroute => isDelivery ? DeliveryStatus.Delivered : null,
                _ => null
            };
        }

        public async Task<OrderDto> UpdateDeliveryStatusAsync(int orderId, DeliveryStatus newStatus, ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (role == "Rider")
            {
                var riderIdClaim = userClaims.FindFirst("RiderId")?.Value;
                if (string.IsNullOrEmpty(riderIdClaim) || order.RiderId != int.Parse(riderIdClaim))
                    throw new Exception("You are not assigned to this order");

                if (order.DeliveryStatus != DeliveryStatus.Enroute || newStatus != DeliveryStatus.Delivered)
                    throw new Exception("Riders can only mark an enroute order as delivered");
            }
            else if (role == "SuperAdmin" || role == "Admin" || role == "Cashier" || role == "MainAdmin")
            {
                if (order.DeliveryStatus == null)
                    throw new Exception("This order is not in an active fulfillment state");

                var expectedNext = GetNextDeliveryStatus(order.DeliveryStatus.Value, order.ServiceType);
                if (expectedNext == null || newStatus != expectedNext.Value)
                    throw new Exception("Orders must move through fulfillment one step at a time");
            }
            else
            {
                throw new Exception("Not authorized to update delivery status");
            }

            order.DeliveryStatus = newStatus;

            await _context.SaveChangesAsync();

            var dto = await GetByIdAsync(order.Id);

            await _hub.Clients.Group("OrderQueue").SendAsync("OrderDeliveryStatusUpdated", dto);

            return dto;
        }

        // Online orders are created unpaid (cash-on-delivery/pickup model). Delivery no longer
        // auto-confirms payment — a delivered order sits visibly "Unpaid" until whoever collected
        // the cash (the rider, from /my-deliveries, or staff from the Queue for pickup orders with
        // no rider) explicitly confirms it here.
        public async Task<OrderDto> ConfirmDeliveryPaymentAsync(int orderId, ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (role == "Rider")
            {
                var riderIdClaim = userClaims.FindFirst("RiderId")?.Value;
                if (string.IsNullOrEmpty(riderIdClaim) || order.RiderId != int.Parse(riderIdClaim))
                    throw new Exception("You are not assigned to this order");
            }
            else if (role != "SuperAdmin" && role != "Admin" && role != "Cashier" && role != "MainAdmin")
            {
                throw new Exception("Not authorized to confirm payment");
            }

            if (order.OrderSource != "Online")
                throw new Exception("Only online orders use this payment-confirmation flow");

            if (order.DeliveryStatus != DeliveryStatus.Delivered)
                throw new Exception("Order must be delivered before payment can be confirmed");

            if (order.Paid)
                throw new Exception("Order is already paid");

            order.Paid = true;
            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.Now;
            order.PaymentMethod ??= "Cash";
            order.CashierUserName = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            await _context.SaveChangesAsync();

            var dto = await GetByIdAsync(order.Id);

            await _hub.Clients.Group("OrderQueue").SendAsync("OrderPaid", order.Id);
            await _hub.Clients.Group("OrderQueue").SendAsync("OrderDeliveryStatusUpdated", dto);

            return dto;
        }

        public async Task<OrderDto> AssignRiderAsync(int orderId, AssignRiderDto dto, ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Cashier" && role != "Admin" && role != "MainAdmin" && role != "SuperAdmin")
                throw new Exception("Only cashier or admin can assign a rider");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.ServiceType != "Delivery")
                throw new Exception("Only delivery orders can be assigned to a rider");

            var rider = await _context.Riders.FirstOrDefaultAsync(r => r.Id == dto.RiderId);
            if (rider == null)
                throw new Exception("Rider not found");
            if (!rider.IsActive)
                throw new Exception("This rider is not active");

            order.RiderId = dto.RiderId;

            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(order.Id);

            await _hub.Clients.Group("OrderQueue").SendAsync("OrderRiderAssigned", result);

            return result;
        }

        public async Task<List<OrderDto>> GetMyDeliveriesAsync(ClaimsPrincipal userClaims)
        {
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Rider")
                throw new Exception("Only riders can view their own deliveries");

            var riderIdClaim = userClaims.FindFirst("RiderId")?.Value;
            if (string.IsNullOrEmpty(riderIdClaim))
                throw new Exception("RiderId missing in token");

            var riderId = int.Parse(riderIdClaim);

            var orders = await _context.Orders
                .Where(o => o.RiderId == riderId && o.Status != OrderStatus.Cancelled)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal).ThenInclude(d => d.DealItems).ThenInclude(di => di.MenuItem)
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .OrderBy(o => o.CreatedAt) // FIFO — oldest first, so riders naturally work the oldest delivery first
                .Take(50)
                .AsNoTracking()
                .ToListAsync();

            return orders.Select(ToOnlineOrderDto).ToList();
        }

        public async Task<bool> ReprintDeliverySlipAsync(int orderId, string copy)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            var items = BuildDeliveryReceiptItems(order);
            var copyType = copy == "kitchen" ? "kitchen" : "customer";

            var dto = new PrintReceiptDto
            {
                CopyType = copyType,
                OrderNo = order.Id,
                Total = order.TotalAmount,
                Discount = 0,
                FinalTotal = order.TotalAmount,
                Items = items,
                OrderTypeLabel = ServiceTypeSlipLabel(order.ServiceType),
                CustomerName = order.CustomerName,
                CustomerPhone = order.PhoneNumber,
                CustomerAddress = order.Address
            };

            var printerType = copyType == "kitchen" ? PrinterKind.Usb2 : PrinterKind.Usb1;
            return _printService.PrintReceipt(dto, printerType);
        }


        public async Task<List<OrderDto>> GetQueuedOrdersAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Queued)
                .Include(o => o.TakenByEmployee)   // Waiter info
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals)
                    .ThenInclude(od => od.Deal)
                .OrderBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Paid = o.Paid,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,
                TakenByEmployeeId = o.TakenByEmployeeId,
                TakenByEmployeeName = o.TakenByEmployee != null ? o.TakenByEmployee.Name : null,
                CustomerName = o.CustomerName,
                PhoneNumber = o.PhoneNumber,
                Address = o.Address,
                OrderSource = o.OrderSource,
                ServiceType = o.ServiceType,
                PaymentMethod = o.PaymentMethod,
                RiderId = o.RiderId,
                RiderName = o.Rider != null ? o.Rider.Name : null,
                RiderCost = o.RiderCost,
                AreaId = o.AreaId,
                AreaName = o.Area != null ? o.Area.Name : null,
                DeliveryFeeCharged = o.DeliveryFeeCharged,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItem.Name,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,

                }).ToList(),
                Deals = o.OrderDeals.Select(od => new OrderDealDto
                {
                    DealId = od.DealId,
                    DealName = od.Deal.DealName,
                    DealPrice = od.DealPrice,
                    Quantity = od.Quantity,
                    Items = od.Deal.DealItems.Select(di => new DealMenuItemDto
                    {
                        MenuItemId = di.MenuItemId,
                        Name = di.MenuItem.Name,
                        Price = di.MenuItem.Price,
                        Quantity = di.Quantity
                    }).ToList()
                }).ToList()
            }).ToList();

            return orderDtos;
        }


        public async Task<OrderDto> PayOrderAsync(
    PayOrderRequest request,
    ClaimsPrincipal userClaims)
        {
            // ================= AUTH =================
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            var employeeIdClaim = userClaims.FindFirst("EmployeeId")?.Value;

            if (role != "Cashier" && role != "Admin" && role != "MainAdmin" && role != "SuperAdmin")
                throw new Exception("Only cashier or admin can pay orders");

            if (role != "Admin" && role != "MainAdmin" && role != "SuperAdmin" && string.IsNullOrEmpty(employeeIdClaim))
                throw new Exception("EmployeeId missing in token");

            int? cashierId = (role == "Admin" || role == "MainAdmin" || role == "SuperAdmin")
                 ? null
                    : int.Parse(employeeIdClaim);

            //int? cashierId = role == "MainAdmin" ? null : int.Parse(employeeIdClaim);

            // ================= GET ORDER =================
            var order = await _context.Orders
                .Include(o => o.TakenByEmployee)
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals)
                    .ThenInclude(od => od.Deal)
                        .ThenInclude(d => d.DealItems)
                            .ThenInclude(di => di.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.Queued)
                throw new Exception("Only queued orders can be paid");

            // ================= PAY =================
            order.Paid = true;
            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.Now;
            order.PaymentMethod = request.PaymentMethod;
            order.CashierId = cashierId;
            order.CashierUserName = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            await _context.SaveChangesAsync();
            await _hub.Clients
                .Group("OrderQueue")
                .SendAsync("OrderPaid", order.Id);


            // ================= RETURN DTO =================
            return new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Paid = order.Paid,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                DeliveryStatus = order.DeliveryStatus,
                TakenByEmployeeId = order.TakenByEmployeeId,
                TakenByEmployeeName = order.TakenByEmployee?.Name,
                CustomerName = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                Latitude = order.Latitude,
                Longitude = order.Longitude,
                OrderSource = order.OrderSource,
                ServiceType = order.ServiceType,
                PaymentMethod = order.PaymentMethod,
                RiderId = order.RiderId,
                RiderName = order.Rider?.Name,
                RiderCost = order.RiderCost,
                AreaId = order.AreaId,
                AreaName = order.Area?.Name,
                DeliveryFeeCharged = order.DeliveryFeeCharged,

                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItem.Name,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList(),

                Deals = order.OrderDeals.Select(od => new OrderDealDto
                {
                    DealId = od.DealId,
                    DealName = od.Deal.DealName,
                    DealPrice = od.DealPrice,
                    Quantity = od.Quantity,
                    Items = od.Deal.DealItems.Select(di => new DealMenuItemDto
                    {
                        MenuItemId = di.MenuItemId,
                        Name = di.MenuItem.Name,
                        Price = di.MenuItem.Price,
                        Quantity = di.Quantity
                    }).ToList()
                }).ToList()
            };
        }


        public async Task<OrderDto> CancelOrderAsync(int orderId, ClaimsPrincipal userClaims)
        {
            // ================= AUTH =================
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            var employeeIdClaim = userClaims.FindFirst("EmployeeId")?.Value;

            var allowedRoles = new[] { "Waiter", "Cashier", "Admin", "MainAdmin", "SuperAdmin" };

            if (!allowedRoles.Contains(role))
                throw new Exception("Unauthorized user");

            bool isAdmin = role == "Admin" || role == "MainAdmin" || role == "SuperAdmin";

            if (!isAdmin && string.IsNullOrEmpty(employeeIdClaim))
                throw new Exception("EmployeeId missing in token");

            int? cancelledByEmployeeId =
                isAdmin ? null : int.Parse(employeeIdClaim);

            // ================= GET ORDER =================
            var order = await _context.Orders
                .Include(o => o.TakenByEmployee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals).ThenInclude(od => od.Deal)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            // ================= VALIDATION =================
            if (order.Paid)
                throw new Exception("Paid orders cannot be cancelled");

            if (order.Status == OrderStatus.Cancelled)
                throw new Exception("Order already cancelled");

            // ================= ROLE-SPECIFIC RULE =================
            if (!isAdmin && role == "Waiter")
            {
                if (order.TakenByEmployeeId != cancelledByEmployeeId)
                    throw new Exception("Waiters can only cancel their own orders");
            }

            // ================= CANCEL =================
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.Now;
            order.CancelledByEmployeeId = cancelledByEmployeeId;
            order.CancelledByUserName = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            await _context.SaveChangesAsync();

            await _hub.Clients
                .Group("OrderQueue")
                .SendAsync("OrderCancelled", order.Id);

            // ================= RETURN DTO =================
            return new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Paid = order.Paid,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                TakenByEmployeeId = order.TakenByEmployeeId,
                TakenByEmployeeName = order.TakenByEmployee?.Name,
                CancelledByUserName = order.CancelledByUserName
            };
        }




        // GET ONE
        public async Task<OrderDto> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.TakenByEmployee)
                .Include(o => o.Cashier)
                .Include(o => o.Rider)
                .Include(o => o.Area)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderDeals)
                    .ThenInclude(od => od.Deal)
                        .ThenInclude(d => d.DealItems)
                            .ThenInclude(di => di.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                throw new Exception("Order not found");
            var itemsTotal = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
            decimal discountAmount = order.TotalAmount < itemsTotal ? itemsTotal - (order.TotalAmount - order.OrderDeals.Sum(od => od.DealPrice * od.Quantity)) : 0;
            return new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Discount = discountAmount,
                Paid = order.Paid,
                Status = order.Status,
                DeliveryStatus = order.DeliveryStatus,
                TakenByEmployeeId = order.TakenByEmployeeId,
                TakenByEmployeeName = order.TakenByEmployee?.Name,
                CashierId = order.CashierId,
                CashierName = order.Cashier?.Name,
                CashierUserName = order.CashierUserName,
                CancelledByUserName = order.CancelledByUserName,
                RejectReason = order.RejectReason,
                CustomerName = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                Latitude = order.Latitude,
                Longitude = order.Longitude,
                OrderSource = order.OrderSource,
                ServiceType = order.ServiceType,
                PaymentMethod = order.PaymentMethod,
                RiderId = order.RiderId,
                RiderName = order.Rider?.Name,
                RiderCost = order.RiderCost,
                AreaId = order.AreaId,
                AreaName = order.Area?.Name,
                DeliveryFeeCharged = order.DeliveryFeeCharged,

                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem?.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),

                Deals = order.OrderDeals.Select(d => new OrderDealDto
                {
                    DealId = d.DealId,
                    DealName = d.Deal.DealName,
                    DealPrice = d.DealPrice,
                    Quantity = d.Quantity,

                    Items = d.Deal.DealItems.Select(x => new DealMenuItemDto
                    {
                        MenuItemId = x.MenuItemId,
                        Name = x.MenuItem.Name,
                        Price = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToList()

                }).ToList()
            };
        }

        public async Task<bool> CancelOrderAsync(CancelOrderRequest request)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.Queued)
                throw new Exception("Only queued orders can be cancelled");

            order.Status = OrderStatus.Cancelled;
            order.CashierId = request.CashierId;

            await _context.SaveChangesAsync();

            return true;
        }


        // GET ALL
        public async Task<List<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.OrderDeals)                     // Include deals
                    .ThenInclude(od => od.Deal)
                        .ThenInclude(d => d.DealItems)
                            .ThenInclude(di => di.MenuItem)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return orders.Select(order => new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem?.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),

                Deals = order.OrderDeals.Select(d => new OrderDealDto
                {
                    DealId = d.DealId,
                    DealName = d.Deal.DealName,
                    DealPrice = d.DealPrice,
                    Quantity = d.Quantity,

                    Items = d.Deal.DealItems.Select(x => new DealMenuItemDto
                    {
                        MenuItemId = x.MenuItemId,
                        Name = x.MenuItem.Name,
                        Price = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToList()
                }).ToList()
            }).ToList();
        }

        public async Task<PagedResult<OrderDto>> GetPagedAsync(int page = 0, int pageSize = 5)
        {
            // 1️⃣ Get total count
            var totalCount = await _context.Orders.CountAsync();

            // 2️⃣ Get paged orders (minimal columns)
            var orders = await _context.Orders
                .OrderByDescending(o => o.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Paid = o.Paid,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,

                    // Use the stored name fields; don't rely on navigation if not loaded
                    TakenByEmployeeId = o.TakenByEmployeeId,
                    TakenByEmployeeName = o.TakenByEmployee.Name,   // store name when order is created

                    CashierId = o.CashierId,
                    CashierName = o.Cashier.Name,                  // store name when cashier marks payment
                    CashierUserName = o.CashierUserName,
                    CancelledByUserName = o.CancelledByUserName,
                    RejectReason = o.RejectReason,
                    OrderSource = o.OrderSource,
                    DeliveryStatus = o.DeliveryStatus,

                    Items = new List<OrderItemDto>(),  // placeholder
                    Deals = new List<OrderDealDto>()   // placeholder
                })
                .AsNoTracking()
                .ToListAsync();

            if (!orders.Any())
                return new PagedResult<OrderDto> { Items = orders, TotalCount = totalCount };

            var orderIds = orders.Select(o => o.Id).ToList();

            // 3️⃣ Load all OrderItems for this page
            var orderItems = await _context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.MenuItemId,
                    MenuItemName = oi.MenuItem.Name,
                    oi.UnitPrice,
                    oi.Quantity
                })
                .AsNoTracking()
                .ToListAsync();

            // 4️⃣ Load all OrderDeals and DealItems for this page
            var orderDeals = await _context.OrderDeals
                .Where(od => orderIds.Contains(od.OrderId))
                .Select(od => new
                {
                    od.OrderId,
                    od.DealId,
                    DealName = od.Deal.DealName,
                    od.DealPrice,
                    od.Quantity,
                    DealItems = od.Deal.DealItems.Select(di => new
                    {
                        di.MenuItemId,
                        di.MenuItem.Name,
                        di.MenuItem.Price,
                        di.Quantity
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            // 5️⃣ Map items and deals to orders
            foreach (var order in orders)
            {
                order.Items = orderItems
                    .Where(i => i.OrderId == order.Id)
                    .Select(i => new OrderItemDto
                    {
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItemName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    })
                    .ToList();

                order.Deals = orderDeals
                    .Where(d => d.OrderId == order.Id)
                    .Select(d => new OrderDealDto
                    {
                        DealId = d.DealId,
                        DealName = d.DealName,
                        DealPrice = d.DealPrice,
                        Quantity = d.Quantity,
                        Items = d.DealItems.Select(di => new DealMenuItemDto
                        {
                            MenuItemId = di.MenuItemId,
                            Name = di.Name,
                            Price = di.Price,
                            Quantity = di.Quantity
                        }).ToList()
                    })
                    .ToList();
            }

            return new PagedResult<OrderDto>
            {
                Items = orders,
                TotalCount = totalCount
            };
        }

        public async Task<List<OrderDto>> GetByDateAsync(DateOnly date)
        {
            var start = BusinessDayHelper.GetStart(date);
            var end = BusinessDayHelper.GetEnd(date);

            return await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Paid = o.Paid,
                    Status = o.Status,
                    TakenByEmployeeId = o.TakenByEmployeeId,
                    TakenByEmployeeName = o.TakenByEmployee != null
                        ? o.TakenByEmployee.Name
                        : null,
                    CashierId = o.CashierId,
                    CashierName = o.Cashier != null
                        ? o.Cashier.Name
                        : null,
                    CustomerName = o.CustomerName,
                    PhoneNumber = o.PhoneNumber,
                    Address = o.Address,
                    OrderSource = o.OrderSource,

                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItem.Name,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    }).ToList(),

                    Deals = o.OrderDeals.Select(d => new OrderDealDto
                    {
                        DealId = d.DealId,
                        DealName = d.Deal.DealName,
                        DealPrice = d.DealPrice,
                        Quantity = d.Quantity
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<PagedResult<OrderDto>> GetPagedAsync(DateOnly? date, int page = 0, int pageSize = 5)
        {
            var query = _context.Orders.AsQueryable();

            // 1️⃣ Filter by date if provided
            if (date.HasValue)
            {
                var start = date.Value.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);

                query = query.Where(o => o.CreatedAt >= start && o.CreatedAt < end);
            }

            // 2️⃣ Get total count AFTER filter
            var totalCount = await query.CountAsync();

            // 3️⃣ Get paged orders
            var orders = await query
                .OrderByDescending(o => o.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Paid = o.Paid,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,

                    TakenByEmployeeId = o.TakenByEmployeeId,
                    TakenByEmployeeName = o.TakenByEmployee != null ? o.TakenByEmployee.Name : null,

                    CashierId = o.CashierId,
                    CashierName = o.Cashier != null ? o.Cashier.Name : null,

                    Items = new List<OrderItemDto>(),
                    Deals = new List<OrderDealDto>()
                })
                .AsNoTracking()
                .ToListAsync();

            if (!orders.Any())
                return new PagedResult<OrderDto> { Items = orders, TotalCount = totalCount };

            var orderIds = orders.Select(o => o.Id).ToList();

            // 4️⃣ Load OrderItems
            var orderItems = await _context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.MenuItemId,
                    MenuItemName = oi.MenuItem.Name,
                    oi.UnitPrice,
                    oi.Quantity
                })
                .AsNoTracking()
                .ToListAsync();

            // 5️⃣ Load Deals
            var orderDeals = await _context.OrderDeals
                .Where(od => orderIds.Contains(od.OrderId))
                .Select(od => new
                {
                    od.OrderId,
                    od.DealId,
                    DealName = od.Deal.DealName,
                    od.DealPrice,
                    od.Quantity,
                    DealItems = od.Deal.DealItems.Select(di => new
                    {
                        di.MenuItemId,
                        di.MenuItem.Name,
                        di.MenuItem.Price,
                        di.Quantity
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            // 6️⃣ Map items + deals
            foreach (var order in orders)
            {
                order.Items = orderItems
                    .Where(i => i.OrderId == order.Id)
                    .Select(i => new OrderItemDto
                    {
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItemName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    })
                    .ToList();

                order.Deals = orderDeals
                    .Where(d => d.OrderId == order.Id)
                    .Select(d => new OrderDealDto
                    {
                        DealId = d.DealId,
                        DealName = d.DealName,
                        DealPrice = d.DealPrice,
                        Quantity = d.Quantity,
                        Items = d.DealItems.Select(di => new DealMenuItemDto
                        {
                            MenuItemId = di.MenuItemId,
                            Name = di.Name,
                            Price = di.Price,
                            Quantity = di.Quantity
                        }).ToList()
                    })
                    .ToList();
            }

            return new PagedResult<OrderDto>
            {
                Items = orders,
                TotalCount = totalCount
            };
        }


    }
}
