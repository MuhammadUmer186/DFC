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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.UseCompatibilityLevel(120))); // DB engine is SQL Server 2014 (12.0) — OPENJSON (used by EF Core 8's default Contains() translation) requires 2016+
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
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapHub<OrderHub>("/hubs/orders");

app.MapControllers();

app.Run();
