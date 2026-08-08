using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAI.Graders;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using RestaurantSystem.Hubs;
using RestaurantSystem.Models;
using System.Security.Claims;

namespace RestaurantSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IKitchenOutService _kitchenOutService;
        public OrderService(ApplicationDbContext context, IHubContext<OrderHub> hub, IKitchenOutService kitchenOutService)
        {
            _context = context;
            _hub = hub;
            _kitchenOutService = kitchenOutService;
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

            var allowedRoles = new[] { "Waiter", "Cashier", "Admin", "MainAdmin" };

            if (!allowedRoles.Contains(userRole))
                throw new Exception("Invalid or unauthorized user");
            int? takenByEmployeeId = null;

            // Admin does not need employee id
            if (userRole != "Admin" && userRole != "MainAdmin")
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

                var order = new Order
                {
                    CreatedAt = DateTime.Now,
                    Paid = isPaid,
                    PaidAt = isPaid ? DateTime.Now : null,
                    Status = isPaid ? OrderStatus.Paid : OrderStatus.Queued,
                    TakenByEmployeeId = isPaid ? takenByEmployeeId : null,
                    CashierId = isPaid ? takenByEmployeeId : null,
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

                // 1️⃣ Collect total menu item quantities
                var menuItemConsumption = new Dictionary<int, decimal>();

                foreach (var oi in order.OrderItems)
                {
                    if (!menuItemConsumption.ContainsKey(oi.MenuItemId))
                        menuItemConsumption[oi.MenuItemId] = 0;

                    menuItemConsumption[oi.MenuItemId] += oi.Quantity;
                }

                // Deals → expand to menu items
                foreach (var od in order.OrderDeals)
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

                // 2️⃣ Resolve recipes → RawItem consumption
                var rawItemConsumption = new Dictionary<int, decimal>();

                foreach (var entry in menuItemConsumption)
                {
                    var menuItemId = entry.Key;
                    var menuQty = entry.Value;

                    var recipes = await _context.MenuRecipes
                        .Where(r => r.MenuItemId == menuItemId)
                        .ToListAsync();

                    //if (!recipes.Any())
                       // throw new Exception($"Menu item {menuItemId} has no recipe defined");

                    foreach (var recipe in recipes)
                    {
                        if (!rawItemConsumption.ContainsKey(recipe.RawItemId))
                            rawItemConsumption[recipe.RawItemId] = 0;

                        rawItemConsumption[recipe.RawItemId] +=
                            recipe.QuantityRequired * menuQty;
                    }
                }

                // 3️⃣ Validate kitchen stock


                // 4️⃣ Deduct stock + log kitchen out
                if (!skipStockCheck)
                {
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
            //using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
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

                var orderDto = await CreateAsync(createOrderRequest, 0, fakeClaims,true);

                // 🔴 GET ACTUAL ENTITY (IMPORTANT)
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderDto.Id);

                // 🟢 SET ONLINE FIELDS
                order.CustomerName = request.CustomerName;
                order.PhoneNumber = request.PhoneNumber;
                order.Address = request.Address;

                order.OrderSource = "Online";
                order.Status = OrderStatus.Queued; // waiting for preparation

                await _context.SaveChangesAsync();

                // 🔔 Notify RMS (REAL-TIME)
                await _hub.Clients
                    .Group("OrderQueue")
                    .SendAsync("NewOnlineOrder", orderDto);

                //await transaction.CommitAsync();

                return orderDto;
            }
            catch
            {
                //await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<List<OrderDto>> GetQueuedOrdersAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Queued)
                .Include(o => o.TakenByEmployee)   // Waiter info
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
                TakenByEmployeeId = o.TakenByEmployeeId,
                TakenByEmployeeName = o.TakenByEmployee != null ? o.TakenByEmployee.Name : null,
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

            if (role != "Cashier" && role != "Admin" && role != "MainAdmin")
                throw new Exception("Only cashier or admin can pay orders");

            if (role != "Admin" && role != "MainAdmin" && string.IsNullOrEmpty(employeeIdClaim))
                throw new Exception("EmployeeId missing in token");

            int? cashierId = (role == "Admin" || role == "MainAdmin")
                 ? null
                    : int.Parse(employeeIdClaim);

            //int? cashierId = role == "MainAdmin" ? null : int.Parse(employeeIdClaim);

            // ================= GET ORDER =================
            var order = await _context.Orders
                .Include(o => o.TakenByEmployee)
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
                TakenByEmployeeId = order.TakenByEmployeeId,
                TakenByEmployeeName = order.TakenByEmployee?.Name,

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

            var allowedRoles = new[] { "Waiter", "Cashier", "Admin", "MainAdmin" };

            if (!allowedRoles.Contains(role))
                throw new Exception("Unauthorized user");

            bool isAdmin = role == "Admin" || role == "MainAdmin";

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
                TakenByEmployeeName = order.TakenByEmployee?.Name
            };
        }




        // GET ONE
        public async Task<OrderDto> GetByIdAsync(int id)
        {
            var order = await _context.Orders
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
                CustomerName = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                OrderSource = order.OrderSource,

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

        public async Task<bool> DeleteAsync(int id)
        {
            // Check if order exists
            var orderExists = await _context.Orders.AnyAsync(o => o.Id == id);
            if (!orderExists)
                throw new Exception("Order not found");

            // 1️⃣ Delete OrderItems related to this order
            var orderItems = _context.OrderItems.Where(oi => oi.OrderId == id);
            _context.OrderItems.RemoveRange(orderItems);

            // 2️⃣ Delete OrderDeals related to this order
            var orderDeals = _context.OrderDeals.Where(od => od.OrderId == id);
            _context.OrderDeals.RemoveRange(orderDeals);

            // 3️⃣ Finally, delete the order itself
            var order = new Order { Id = id }; // attach dummy entity
            _context.Orders.Attach(order);
            _context.Orders.Remove(order);

            // 4️⃣ Save changes
            await _context.SaveChangesAsync();

            return true;
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
