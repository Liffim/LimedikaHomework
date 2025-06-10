using Microsoft.EntityFrameworkCore;
using LimedikaWebApp.Data;
using LimedikaWebApp.Models;
using LimedikaWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<PostItApiSettings>(builder.Configuration.GetSection("PostItApiSettings"));

builder.Services.AddHttpClient();

builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<ClientImportService>();
builder.Services.AddScoped<PostCodeUpdateServices>();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
   
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
