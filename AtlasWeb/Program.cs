
using AtlasWeb.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// ATLAS SOFT - HIZ SINIRLANDIRMA
// ==========================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // DEMO TALEP LIMITI
    // 10 dakikada en fazla 3 talep
    options.AddPolicy("DemoTalepLimiti", context =>
    {
        var ip = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ATLAS SOFT SATIS DANISMANI
    // 10 dakikada en fazla 20 mesaj
    options.AddPolicy("SalesChatLimiti", context =>
    {
        var ip = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

// ==========================================
// SERVISLER
// ==========================================

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AtlasSalesService>();

// ==========================================
// MVC
// ==========================================

builder.Services.AddControllersWithViews();

// ==========================================
// SESSION
// ==========================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.SameAsRequest;
});

// ==========================================
// UYGULAMAYI OLUSTUR
// ==========================================

var app = builder.Build();

// ==========================================
// HATA YONETIMI
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // HTTPS kullandigimiz icin etkin
    app.UseHsts();
}

// HTTPS yonlendirmesi
app.UseHttpsRedirection();

// ==========================================
// MIDDLEWARE
// ==========================================

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseRateLimiter();

app.UseAuthorization();

// ==========================================
// CONTROLLER ROUTING
// ==========================================

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// ATLAS SOFT BASLAT
// ==========================================

app.Run();
