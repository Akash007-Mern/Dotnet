using Dotnet.Models;
using Dotnet.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext with connection string
builder.Services.AddDbContext<CrudContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbConn"))
        .EnableSensitiveDataLogging());

//Register DataSecurityProvider
builder.Services.AddSingleton<DataSecurityProvider>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllerRoute(

    name: "default",
    pattern: "{controller=Static}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
