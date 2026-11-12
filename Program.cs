using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.Models;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DragneelWedding.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = "/TaiKhoan/Login";
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<KetNoiDatabase>();
builder.Services.AddScoped<LoaiSanPhamRepository>();
builder.Services.AddScoped<SanPhamRepository>();
builder.Services.AddScoped<GioThueRepository>();
builder.Services.AddScoped<TaiKhoanRepository>();
builder.Services.AddSingleton<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();
builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();
builder.Services.AddScoped<IGioThueService, GioThueService>();
builder.Services.AddScoped<ITrangChuService, TrangChuService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
