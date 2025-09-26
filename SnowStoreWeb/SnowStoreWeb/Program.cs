using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Models;

var builder = WebApplication.CreateBuilder(args);
// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SnowStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SnowStoreConnection")));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
//Add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
