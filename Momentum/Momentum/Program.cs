using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));



var app = builder.Build();
app.MapControllerRoute("area", "{area:exists}/{controller=dashboard}/{action=index}/{id?}");
app.MapControllerRoute("default", "{controller=home}/{action=index}/{id?}");
app.UseStaticFiles();
app.Run();
