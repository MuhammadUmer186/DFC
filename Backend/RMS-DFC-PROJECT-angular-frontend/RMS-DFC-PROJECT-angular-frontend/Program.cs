using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Polly;
using RestaurantSystem.Hubs;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Interfaces;
using RestaurantSystem.Services;
using System;
using System.Security.Claims;
using System.Text;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

// Program.cs
//string geminiApiKey = builder.Configuration["Gemini:ApiKey"]!;

//// -------------------------------
//// 2️⃣ Resilient HttpClient for Gemini
//// -------------------------------
//builder.Services.AddHttpClient("GeminiClient", client =>
//{
//    client.Timeout = TimeSpan.FromSeconds(120); // 2 minutes
//})
//.AddStandardResilienceHandler(options =>
//{
//    // Retry policy
//    options.Retry.MaxRetryAttempts = 3;
//    options.Retry.Delay = TimeSpan.FromSeconds(3);
//    options.Retry.BackoffType = DelayBackoffType.Exponential;

//    // Timeout policy (Polly)
//    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(120); // Matches HttpClient timeout
//});



//// === Register Semantic Kernel + Plugins ===
//builder.Services.AddScoped(sp =>
//{
//    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
//    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
//    var resilientClient = httpClientFactory.CreateClient("GeminiClient");

//    var kernelBuilder = Kernel.CreateBuilder();

//    // Gemini Chat Completion
//    kernelBuilder.AddGoogleAIGeminiChatCompletion(
//        modelId: "gemini-2.5-flash",
//        apiKey: geminiApiKey,
//        httpClient: resilientClient
//    );

//    // Register your plugins
//    kernelBuilder.Plugins.AddFromObject(new SqlAgentPlugin(dbContext));
//    kernelBuilder.Plugins.AddFromObject(new RestaurantDataPlugin(dbContext));

//    return kernelBuilder.Build();
//});
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

app.UseRouting();

app.UseCors(MyCors);

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<OrderHub>("/hubs/orders");

app.MapControllers();

app.Run();
