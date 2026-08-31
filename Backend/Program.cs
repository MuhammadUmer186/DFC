using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using RestaurantSystem.Hubs;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Interfaces;
using RestaurantSystem.Services;
using RestaurantSystem.Services.Ai;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddSignalR();

// Offline-first / cloud-sync — Phase 2. Ambient node identity + the SaveChanges
// interceptor that stamps GlobalId / AggregateVersion / UTC timestamps and
// writes delete tombstones.
builder.Services.AddSingleton<RestaurantSystem.Sync.INodeContext, RestaurantSystem.Sync.NodeContext>();
builder.Services.AddScoped<RestaurantSystem.Sync.SyncStampingInterceptor>();
builder.Services.AddScoped<RestaurantSystem.Sync.AggregateSnapshotService>();
builder.Services.AddScoped<RestaurantSystem.Sync.SyncBackfillService>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.UseCompatibilityLevel(120)) // DB engine is SQL Server 2014 (12.0) — OPENJSON (used by EF Core 8's default Contains() translation) requires 2016+
    .AddInterceptors(sp.GetRequiredService<RestaurantSystem.Sync.SyncStampingInterceptor>()));
// ===== Offline-first / cloud-sync — Phase 1 (node & branch identity) =====
// Real values come from the Deployment section via env vars / secret mounts
// (Deployment__NodeId, Deployment__NodeRole, Deployment__BranchId, ...).
var deploymentOptions = builder.Configuration
    .GetSection(RestaurantSystem.Sync.DeploymentOptions.SectionName)
    .Get<RestaurantSystem.Sync.DeploymentOptions>() ?? new RestaurantSystem.Sync.DeploymentOptions();
builder.Services.AddSingleton(deploymentOptions);

// ===== Offline-first / cloud-sync — Phase 14 (controlled migrations) =====
var migratorOptions = builder.Configuration
    .GetSection(RestaurantSystem.Sync.MigratorOptions.SectionName)
    .Get<RestaurantSystem.Sync.MigratorOptions>() ?? new RestaurantSystem.Sync.MigratorOptions();
builder.Services.AddSingleton(migratorOptions);
builder.Services.AddScoped<RestaurantSystem.Sync.DatabaseMigrator>();

// ===== Offline-first / cloud-sync — Phase 5 (transactional sync engine) =====
var syncOptions = builder.Configuration
    .GetSection(RestaurantSystem.Sync.SyncOptions.SectionName)
    .Get<RestaurantSystem.Sync.SyncOptions>() ?? new RestaurantSystem.Sync.SyncOptions();
builder.Services.AddSingleton(syncOptions);
builder.Services.AddScoped<RestaurantSystem.Sync.SyncApplyService>();
builder.Services.AddHttpClient<RestaurantSystem.Sync.SyncPeerClient>(c =>
    c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHostedService<RestaurantSystem.Sync.SyncWorker>();

// ===== Offline-first / cloud-sync — Phase 11 (public online-order flags) =====
builder.Services.Configure<RestaurantSystem.Sync.PublicOrderingOptions>(
    builder.Configuration.GetSection(RestaurantSystem.Sync.PublicOrderingOptions.SectionName));
builder.Services.AddScoped<RestaurantSystem.Sync.EdgeConnectivity>();

// ===== Offline-first / cloud-sync — Phase 12 (uploaded-media metadata) =====
builder.Services.Configure<RestaurantSystem.Sync.UploadOptions>(
    builder.Configuration.GetSection(RestaurantSystem.Sync.UploadOptions.SectionName));
builder.Services.AddScoped<RestaurantSystem.Sync.UploadStore>();
builder.Services.AddScoped<RestaurantSystem.Sync.UploadedFileBackfillService>();

// ===== Offline-first / cloud-sync — Phase 17 (health) =====
builder.Services.AddHealthChecks()
    .AddCheck<RestaurantSystem.Sync.DatabaseHealthCheck>("database", tags: new[] { "ready" });

// ===== Offline-first / cloud-sync — Phase 6 (idempotent commands) =====
var idempotencyOptions = builder.Configuration
    .GetSection(RestaurantSystem.Sync.IdempotencyOptions.SectionName)
    .Get<RestaurantSystem.Sync.IdempotencyOptions>() ?? new RestaurantSystem.Sync.IdempotencyOptions();
builder.Services.AddSingleton(idempotencyOptions);
builder.Services.AddScoped<RestaurantSystem.Sync.ICommandContext, RestaurantSystem.Sync.CommandContext>();

builder.Services.AddScoped<RestaurantSystem.Sync.NodeRegistrationService>(sp =>
    new RestaurantSystem.Sync.NodeRegistrationService(
        sp.GetRequiredService<ApplicationDbContext>(),
        sp.GetRequiredService<RestaurantSystem.Sync.DeploymentOptions>(),
        sp.GetRequiredService<ILogger<RestaurantSystem.Sync.NodeRegistrationService>>(),
        sp.GetRequiredService<IHostEnvironment>().ContentRootPath));

builder.Services.AddScoped<IRestaurantClock, RestaurantClock>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IRawItemService, RawItemService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IKitchenOutService, KitchenOutService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProfitService, ProfitService>();
builder.Services.AddScoped<IConsumptionService, ConsumptionService>();
builder.Services.AddScoped<IMenuProfitService, MenuProfitService>();
builder.Services.AddScoped<IStockReportService, StockReportService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderNumberService, OrderNumberService>(); // Phase 3
builder.Services.AddScoped<IStockLedger, StockLedger>();               // Phase 4
builder.Services.AddScoped<RestaurantSystem.Sync.StockLedgerBackfillService>(); // Phase 4
builder.Services.AddScoped<IVendorAccountService, VendorAccountService>();
builder.Services.AddScoped<IWasteService, WasteService>();
builder.Services.AddScoped<IUtilityBillService, UtilityBillService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddSingleton<Printing.Services.PrintService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<IReportService,ReportService>();
builder.Services.AddScoped<IMenuRecipeService, MenuRecipeService>();
builder.Services.AddScoped<IRiderService, RiderService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISystemMaintenanceService, SystemMaintenanceService>();

// ===== AI foundation =====
builder.Services.Configure<AiFeatureOptions>(builder.Configuration.GetSection("AiFeatures"));
builder.Services.Configure<AiProviderOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddScoped<IAiAuditService, AiAuditService>();
builder.Services.AddSingleton<MockAiProvider>();
builder.Services.AddSingleton<OpenAiProvider>();
builder.Services.AddScoped<IAiProvider>(sp =>
{
    var features = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiFeatureOptions>>().Value;
    var providerOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiProviderOptions>>().Value;
    // Falls back to the mock provider whenever there's no real key configured, so the app
    // never crashes on missing config — AI features just answer with an explicit placeholder.
    return (features.UseMockProvider || string.IsNullOrWhiteSpace(providerOptions.ApiKey))
        ? sp.GetRequiredService<MockAiProvider>()
        : sp.GetRequiredService<OpenAiProvider>();
});
builder.Services.AddScoped<ForecastingService>();
builder.Services.AddScoped<InventoryRecommendationService>();
builder.Services.AddScoped<IInsightsTools, InsightsTools>();
builder.Services.AddScoped<InsightsAssistantService>();
builder.Services.AddHostedService<ForecastBackgroundService>();

builder.Services.AddRateLimiter(options =>
{
    // Applied via [EnableRateLimiting("ai")] on the AI controllers — a live LLM call costs
    // real money per request, so this is a blunt but necessary abuse guard.
    options.AddFixedWindowLimiter("ai", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// Phase 8: JWT signing/validation material — RS256 per node (if configured) plus
// legacy HS256, trusting BOTH the cloud and edge issuers.
var authKeys = new RestaurantSystem.Sync.AuthKeyProvider(
    builder.Configuration,
    LoggerFactory.Create(b => b.AddConsole()).CreateLogger<RestaurantSystem.Sync.AuthKeyProvider>());
builder.Services.AddSingleton(authKeys);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Only for development
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,
        ValidateLifetime = true,
        ValidIssuers = authKeys.ValidIssuers,                         // cloud + edge
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        IssuerSigningKeys = authKeys.ValidationKeys                    // HS256 + all trusted public keys
    };
});
builder.Services.AddAuthorization();
// Auth & Policy setup (optional customization)
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
//});
var MyCors = "AllowLocalAndFrontends";

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyCors, policy =>
        policy
            .WithOrigins(
                "http://localhost",
                "http://localhost:4200",
                "http://localhost:4300",
                "http://192.168.18.155",
                "http://192.168.18.155:4200",
                "http://192.168.0.101:4200",
                "http://192.168.18.10:4200",
                "http://192.168.18.10:4300",
                "http://192.168.100.37:4200",
                "https://public-rms.web.app",
                "https://public-rms.firebaseapp.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

//builder.WebHost.ConfigureKestrel(options =>
//{
    // HTTP for testing (optional)
    //options.ListenAnyIP(8080);

    // HTTPS
    //options.ListenAnyIP(7122, listenOptions =>
    //{
    //    listenOptions.UseHttps("C:\\temp\\localcert.pfx", "password123");
    //});
//});

var app = builder.Build();

// ===== Offline-first / cloud-sync — Phase 14: one-shot controlled migrator =====
// `dotnet RestaurantSystem.dll --migrate` (or env RUN_MIGRATOR=true) runs the
// migrator and exits WITHOUT starting Kestrel. The production API container is
// gated on this finishing successfully (docker compose `service_completed_successfully`).
if (args.Contains("--migrate") ||
    string.Equals(Environment.GetEnvironmentVariable("RUN_MIGRATOR"), "true", StringComparison.OrdinalIgnoreCase))
{
    using var migScope = app.Services.CreateScope();
    var migrator = migScope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.DatabaseMigrator>();
    return await migrator.RunAsync();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var migOpts = scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.MigratorOptions>();
    var startupLog = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Phase 14: no unconditional Migrate() on API start.
    //  - Development (or Migrator:AutoMigrate=true): apply, as before, for convenience.
    //  - Otherwise: verify only. Pending migrations => fail fast (Migrator:RequireUpToDate,
    //    default true outside Development) so a misconfigured deploy never runs on an old schema.
    var autoMigrate = migOpts.AutoMigrate ?? env.IsDevelopment();
    if (autoMigrate)
    {
        db.Database.Migrate();
    }
    else
    {
        var pending = db.Database.GetPendingMigrations().ToList();
        if (pending.Count > 0)
        {
            var msg = $"Phase14: {pending.Count} pending migration(s): {string.Join(", ", pending)}. " +
                      "Run the migrator container ('--migrate') before starting the API.";
            var requireUpToDate = migOpts.RequireUpToDate ?? !env.IsDevelopment();
            if (requireUpToDate)
                throw new InvalidOperationException(msg);
            startupLog.LogCritical(msg);
        }
    }

    // Offline-first / cloud-sync — Phase 1. Idempotent; only ever inserts/refreshes
    // the new Branch / SystemNode / NodeHeartbeat rows. A failure here must not stop
    // the API from serving (sync is additive and can be repaired later).
    try
    {
        var registration = scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.NodeRegistrationService>();
        var identity = await registration.EnsureRegisteredAsync();

        // Phase 2: publish identity to the ambient context the interceptor reads,
        // then backfill origin/branch on rows created before sync existed.
        scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.INodeContext>().Set(identity);
        await scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.SyncBackfillService>().RunAsync();

        // Phase 4: seed the inventory ledger's opening balances from StoreStock.
        await scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.StockLedgerBackfillService>().RunAsync();

        // Phase 12: index pre-existing wwwroot/uploads files into UploadedFiles.
        await scope.ServiceProvider.GetRequiredService<RestaurantSystem.Sync.UploadedFileBackfillService>().RunAsync();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "Sync/Phase1-2: node registration / backfill failed; continuing startup.");
    }
}
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "swagger";
});


//app.UseHttpsRedirection();

app.UseStaticFiles(); // serves wwwroot/uploads/... (category images, etc.)

app.UseRouting();

app.UseCors(MyCors);

// Phase 5: HMAC gate for the node-to-node sync channel (/api/sync/*).
app.UseMiddleware<RestaurantSystem.Sync.SyncHmacMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Phase 6: replay/short-circuit mutating requests that carry an Idempotency-Key.
// After auth so the caller is known; before endpoints so it wraps their execution.
app.UseMiddleware<RestaurantSystem.Sync.IdempotencyMiddleware>();

app.UseRateLimiter();
app.MapHub<OrderHub>("/hubs/orders");

app.MapControllers();

// Phase 17: health endpoints (safe details only — no secrets/exceptions in the body).
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // liveness = process is up and serving
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready")
});

app.Run();

// Phase 14: the `--migrate` branch above returns an exit code, so the top-level
// entry point is `Task<int>` and every path must return. app.Run() blocks until
// shutdown; this is reached only on a clean stop.
return 0;
