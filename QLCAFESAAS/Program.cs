using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLCAFESAAS.Models.Repository;
using QLCAFESAAS.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services 
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CafeRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<PaymentRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policy => policy.RequireRole("admin"))
    .AddPolicy("manager", policy => policy.RequireRole("manager"))
    .AddPolicy("staff", policy => policy.RequireRole("staff"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian h?t h?n Session (30 ph�t)
    options.Cookie.HttpOnly = true; // Ch? s? d?ng HTTP (an to�n h?n)
    options.Cookie.IsEssential = true; // Cookie lu�n ???c l?u (b?t bu?c cho Session)
});



// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
