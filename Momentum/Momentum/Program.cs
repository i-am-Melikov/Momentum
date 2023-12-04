using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Hubs.SignalRHubs;
using Momentum.Interfaces;
using Momentum.Models;
using Momentum.Services;
using Momentum.ViewModels;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;

    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SmtpSetting>(builder.Configuration.GetSection("SmtpSetting"));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddSignalR();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ContactHub>("/contact");
});
app.MapControllerRoute("area", "{area:exists}/{controller=dashboard}/{action=index}/{id?}");
app.MapControllerRoute("default", "{controller=home}/{action=index}/{id?}");
app.UseStaticFiles();
app.UseSession();
app.Run();
