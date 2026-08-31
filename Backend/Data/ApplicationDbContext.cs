using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // See UtcDateTimeConverters.cs for why this exists — every DateTime read from the
            // database is stamped Kind=Utc so it round-trips through JSON correctly.
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<WasteRecord> WasteRecords { get; set; }
        public DbSet<WasteItem> WasteItems { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<DealItem> DealItems { get; set; }
        public DbSet<UtilityBill> UtilityBills { get; set; }
        public DbSet<OrderDeal> OrderDeals { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<RawItem> RawItems { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<StoreStock> StoreStocks { get; set; }
        public DbSet<KitchenOut> KitchenOuts { get; set; }
        public DbSet<KitchenOutItem> KitchenOutItems { get; set; }
        public DbSet<MenuRecipe> MenuRecipes { get; set; }
        public DbSet<VendorPayment> VendorPayments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SalaryPayment> SalaryPayments { get; set; }
        public DbSet<ServiceTimeSetting> ServiceTimeSettings { get; set; } = null!;
        public DbSet<SiteSetting> SiteSettings { get; set; } = null!;
        public DbSet<Rider> Riders { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;

        // ===== AI feature tables =====
        public DbSet<AiAuditLog> AiAuditLogs { get; set; } = null!;
        public DbSet<AiForecastRun> AiForecastRuns { get; set; } = null!;
        public DbSet<AiForecastValue> AiForecastValues { get; set; } = null!;
        public DbSet<AiInventoryRecommendation> AiInventoryRecommendations { get; set; } = null!;
        public DbSet<AiInventoryRecommendationDecision> AiInventoryRecommendationDecisions { get; set; } = null!;
        public DbSet<AiConversation> AiConversations { get; set; } = null!;
        public DbSet<AiMessageRecord> AiMessageRecords { get; set; } = null!;
        public DbSet<AiToolExecutionRecord> AiToolExecutionRecords { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;

        // ===== Offline-first / cloud-sync — Phase 1 (node & branch identity) =====
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<SystemNode> SystemNodes { get; set; } = null!;
        public DbSet<NodeHeartbeat> NodeHeartbeats { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureCategory(modelBuilder);
            ConfigureMenuItem(modelBuilder);
            ConfigureOrder(modelBuilder);
            ConfigureOrderItem(modelBuilder);
            ConfigureVendor(modelBuilder);
            ConfigureRawItem(modelBuilder);
            ConfigurePurchaseOrder(modelBuilder);
            ConfigurePurchaseOrderItem(modelBuilder);
            ConfigureStoreStock(modelBuilder);
            ConfigureKitchenOut(modelBuilder);
            ConfigureKitchenOutItem(modelBuilder);
            ConfigureMenuRecipe(modelBuilder);
            ConfigureOrderDeals(modelBuilder);
            ConfigureServiceTimeSetting(modelBuilder);
            ConfigureSiteSetting(modelBuilder);
            ConfigureRider(modelBuilder);
            ConfigureArea(modelBuilder);
            ConfigureAiEntities(modelBuilder);
            ConfigureSyncNodeIdentity(modelBuilder);

            // Seed Vendors

        }

        // Offline-first / cloud-sync — Phase 1. Additive: three new tables, no
        // change to any existing entity mapping.
        private void ConfigureSyncNodeIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasIndex(e => e.BranchId).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(32);
            });

            modelBuilder.Entity<SystemNode>(entity =>
            {
                entity.HasIndex(e => e.NodeId).IsUnique();
                entity.HasIndex(e => e.BranchId);
                // Human-readable across nodes — never compared as an ordinal.
                entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(16);
                entity.Property(e => e.DisplayName).HasMaxLength(200);
                entity.Property(e => e.BaseUrl).HasMaxLength(512);
                entity.Property(e => e.AppVersion).HasMaxLength(64);
                entity.Property(e => e.SchemaVersion).HasMaxLength(128);
            });

            modelBuilder.Entity<NodeHeartbeat>(entity =>
            {
                entity.HasIndex(e => new { e.NodeId, e.ReceivedAtUtc });
                entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(16);
                entity.Property(e => e.AppVersion).HasMaxLength(64);
                entity.Property(e => e.SchemaVersion).HasMaxLength(128);
                entity.Property(e => e.Source).HasMaxLength(64);
            });
        }

        private void ConfigureAiEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AiAuditLog>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Feature);
            });

            modelBuilder.Entity<AiForecastRun>(entity =>
            {
                entity.HasIndex(e => new { e.ForecastFrom, e.ForecastTo });
            });

            modelBuilder.Entity<AiForecastValue>(entity =>
            {
                entity.HasOne(v => v.ForecastRun)
                    .WithMany(r => r.Values)
                    .HasForeignKey(v => v.ForecastRunId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.MenuItem)
                    .WithMany()
                    .HasForeignKey(v => v.MenuItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.ForecastDate, e.MenuItemId, e.HourOfDay });
            });

            modelBuilder.Entity<AiInventoryRecommendation>(entity =>
            {
                entity.HasOne(r => r.RawItem)
                    .WithMany()
                    .HasForeignKey(r => r.RawItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.RawItemId, e.Status });
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<AiInventoryRecommendationDecision>(entity =>
            {
                entity.HasOne(d => d.Recommendation)
                    .WithMany(r => r.Decisions)
                    .HasForeignKey(d => d.RecommendationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AiConversation>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AiMessageRecord>(entity =>
            {
                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AiToolExecutionRecord>(entity =>
            {
                entity.HasIndex(e => e.ConversationId);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
            });
        }

        // ====== CONFIGURE ENTITIES ======

        private void ConfigureMenuRecipe(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuRecipe>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.QuantityRequired)
                      .HasPrecision(12, 4);

                entity.HasOne(m => m.MenuItem)
                      .WithMany(m => m.MenuRecipes)  // collection in MenuItem
                      .HasForeignKey(m => m.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.RawItem)
                      .WithMany(r => r.MenuRecipes)  // collection in RawItem
                      .HasForeignKey(m => m.RawItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }

        private void ConfigureOrderDeals(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDeal>(entity =>
            {
                entity.HasKey(od => od.Id);

                entity.Property(od => od.DealPrice)
                      .HasColumnType("decimal(12,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDeals)
                      .HasForeignKey(od => od.OrderId);

                entity.HasOne(od => od.Deal)
                      .WithMany(d => d.OrderDeals)  // Add collection in Deal
                      .HasForeignKey(od => od.DealId);
            });

        }

        private void ConfigureKitchenOutItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KitchenOutItem>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Quantity)
                      .HasColumnType("decimal(12,4)");

                entity.HasOne(i => i.RawItem)
                      .WithMany(r => r.KitchenOutItems)
                      .HasForeignKey(i => i.RawItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.KitchenOut)
                      .WithMany(k => k.KitchenOutItems)
                      .HasForeignKey(i => i.KitchenOutId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureKitchenOut(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KitchenOut>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.HasMany(k => k.KitchenOutItems)
                      .WithOne(i => i.KitchenOut)
                      .HasForeignKey(i => i.KitchenOutId);
            });
        }

        private void ConfigureStoreStock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreStock>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Quantity)
                      .HasColumnType("decimal(12,2)");

                entity.HasIndex(s => new { s.RawItemId, s.VendorId })
                      .IsUnique();

                entity.HasOne(s => s.RawItem)
                      .WithMany(r => r.StoreStocks)
                      .HasForeignKey(s => s.RawItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Vendor)
                      .WithMany(v => v.StoreStocks)
                      .HasForeignKey(s => s.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurePurchaseOrderItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.UnitPrice)
                      .HasColumnType("decimal(12,2)");

                entity.Property(i => i.Quantity)
                      .HasColumnType("decimal(12,2)");

                entity.Property(i => i.TotalPrice)
                      .HasColumnType("decimal(12,2)");

                entity.HasOne(i => i.RawItem)
                      .WithMany(r => r.PurchaseOrderItems)
                      .HasForeignKey(i => i.RawItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.PurchaseOrder)
                      .WithMany(po => po.PurchaseOrderItems)
                      .HasForeignKey(i => i.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigurePurchaseOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(po => po.Id);

                entity.Property(po => po.BillNo)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(po => po.TotalAmount)
                      .HasColumnType("decimal(12,2)");

                entity.HasOne(po => po.Vendor)
                      .WithMany(v => v.PurchaseOrders)
                      .HasForeignKey(po => po.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(po => po.PurchaseOrderItems)
                      .WithOne(i => i.PurchaseOrder)
                      .HasForeignKey(i => i.PurchaseOrderId);
            });
        }

        private void ConfigureRawItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawItem>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(r => r.Unit)
                      .IsRequired()
                      .HasMaxLength(50);
            });
        }

        private void ConfigureVendor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasMany(v => v.PurchaseOrders)
                      .WithOne(po => po.Vendor)
                      .HasForeignKey(po => po.VendorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(v => v.StoreStocks)
                      .WithOne(s => s.Vendor)
                      .HasForeignKey(s => s.VendorId);
            });
        }

        private static void ConfigureOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(12,2)")
                      .IsRequired();

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.Latitude)
                      .HasColumnType("decimal(9,6)");

                entity.Property(o => o.Longitude)
                      .HasColumnType("decimal(9,6)");

                entity.Property(o => o.RiderCost)
                      .HasColumnType("decimal(12,2)");

                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.TakenByEmployee)
                      .WithMany()
                      .HasForeignKey(o => o.TakenByEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Cashier)
                      .WithMany()
                      .HasForeignKey(o => o.CashierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Rider)
                      .WithMany(r => r.Orders)
                      .HasForeignKey(o => o.RiderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.DeliveryFeeCharged)
                      .HasColumnType("decimal(12,2)");

                entity.HasOne(o => o.Area)
                      .WithMany()
                      .HasForeignKey(o => o.AreaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.OrderDeals)
                      .WithOne(od => od.Order)
                      .HasForeignKey(od => od.OrderId);
            });
        }

        private static void ConfigureRider(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rider>(entity =>
            {
                entity.ToTable("Riders");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name).IsRequired();
                entity.Property(r => r.PhoneNumber).IsRequired();

                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Rider)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RiderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureArea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Areas");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(a => a.DeliveryFee)
                      .HasColumnType("decimal(12,2)");

                entity.HasIndex(a => a.Name).IsUnique();

                var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                entity.HasData(
                    new Area { Id = 1, Name = "Unknown", DeliveryFee = 100, IsActive = true, CreatedAt = seedDate }
                );
            });
        }

        private static void ConfigureServiceTimeSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceTimeSetting>(entity =>
            {
                entity.ToTable("ServiceTimeSettings");

                entity.HasKey(s => s.Id);

                entity.HasIndex(s => s.ServiceType).IsUnique();

                entity.Property(s => s.ServiceType)
                      .HasMaxLength(20)
                      .IsRequired();

                var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                entity.HasData(
                    new ServiceTimeSetting { Id = 1, ServiceType = "DineIn", MinMinutes = 15, MaxMinutes = 20, IsEnabled = true, UpdatedAt = seedDate },
                    new ServiceTimeSetting { Id = 2, ServiceType = "Takeaway", MinMinutes = 15, MaxMinutes = 20, IsEnabled = true, UpdatedAt = seedDate },
                    new ServiceTimeSetting { Id = 3, ServiceType = "Delivery", MinMinutes = 25, MaxMinutes = 35, IsEnabled = true, UpdatedAt = seedDate }
                );
            });
        }

        private static void ConfigureSiteSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteSetting>(entity =>
            {
                entity.ToTable("SiteSettings");

                entity.HasKey(s => s.Id);

                entity.HasData(new SiteSetting { Id = 1, HeroImageUrl = null });
            });
        }

        private static void ConfigureOrderItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.Quantity)
                      .HasColumnType("decimal(12,2)");

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(12,2)");

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId);

                entity.HasOne(oi => oi.MenuItem)
                      .WithMany(m => m.OrderItems)
                      .HasForeignKey(oi => oi.MenuItemId);
            });
        }

        private static void ConfigureMenuItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.ToTable("MenuItems");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(m => m.Price)
                      .HasColumnType("decimal(12,2)")
                      .IsRequired();

                entity.HasOne(m => m.Category)
                      .WithMany(c => c.MenuItems)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasMany(c => c.MenuItems)
                      .WithOne(m => m.Category)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
