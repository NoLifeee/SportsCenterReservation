using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SportsCenterReservation.Data;
using SportsCenterReservation.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Adauga configurare pentru TempData
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = null; // sesiune de tip "non-persistent cookie"
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Creeaza baza de date daca nu exista
    context.Database.EnsureCreated();

    // Adauga utilizatori
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Username = "admin", Password = "admin", Role = "Admin" },
            new User { Username = "user1", Password = "1234", Role = "User" }
        );
        context.SaveChanges();
    }

    // Adauga servicii
    if (!context.Servicii.Any())
    {
        context.Servicii.AddRange(
            new Serviciu { Nume = "Tenis" },
            new Serviciu { Nume = "Fotbal" },
            new Serviciu { Nume = "Karate" },
            new Serviciu { Nume = "Culturism" },
            new Serviciu { Nume = "Fitness" },
            new Serviciu { Nume = "Judo" },
            new Serviciu { Nume = "Box" },
            new Serviciu { Nume = "Aikido" },
            new Serviciu { Nume = "Basket" }
        );
        context.SaveChanges();
    }
}

app.Run();
