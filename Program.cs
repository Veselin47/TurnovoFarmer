using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurnovoFarmer.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Конфигуриране на Connection String към базата данни
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Добавяне на поддръжка за Сесии (Session) и Кеш - ЗАДЪЛЖИТЕЛНО ЗА КОЛИЧКАТА
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Количката се пази 30 минути
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Конфигуриране на ASP.NET Core Identity с поддръжка на Роли
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Сложи го на true по-късно, когато пуснеш имейл услуга
    options.Password.RequireDigit = true;           // Базова сигурност за паролите
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>() // Позволява управлението на роли (Клиент, Фермер, Админ)
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 4. Добавяне на поддръжка за Контролери и Изгледи (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигуриране на HTTP request pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// .NET 10 / .NET 9 Оптимизация за статични файлове (CSS, JS, Снимки)
app.MapStaticAssets();

app.UseRouting();

// 5. ВАЖНО: Активиране на сесиите в Middleware конвейера (Трябва да е ПРЕДИ Authentication/Authorization)
app.UseSession();

// Проверка кой си и какво имаш право да правиш
app.UseAuthentication();
app.UseAuthorization();

// Рутиране на MVC контролерите
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Поддръжка за Identity страниците по подразбиране (Login/Register)
app.MapRazorPages()
   .WithStaticAssets();

// 6. ИЗВИКВАНЕ НА СИЙДНАТИТЕ ДАННИ (SEED DATA) ПРИ СТАРТИРАНЕ
// Този блок проверява дали базата е празна и инжектира продуктите на чичо ти автоматично
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.SeedData();

        await IdentitySeedService.SeedRolesAndUsersAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Възникна грешка при първоначалното напълване (Seeding) на базата данни.");
    }
}

app.Run();