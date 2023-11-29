using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;

var builder = WebApplication.CreateBuilder(args);

// DB https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore/8.0.0
var connectionString = builder.Configuration.GetConnectionString("ToDoAndNotes3") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<TdnDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.UI/8.0.0
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TdnDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
});

var app = builder.Build();

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
//app.MapControllerRoute(
//    name: "account",
//    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();


// Migrations https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools/8.0.0