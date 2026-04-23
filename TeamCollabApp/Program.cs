using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TeamCollabApp.Data;
using TeamCollabApp.Models;
using TeamCollabApp.Services;
using TeamCollabApp.HttpClients;
using TeamCollabApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<GuestSessionOptions>(builder.Configuration.GetSection("GuestSession"));
builder.Services.AddScoped<IGuestSessionService, GuestSessionService>();

builder.Services.AddHttpClient<TasksBoardClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:TasksApiUrl"]!));

builder.Services.AddHttpClient<SearchClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:SearchApiUrl"]!));

builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = new DatabaseSeeder(scope.ServiceProvider);
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.MapHub<CollaborationHub>("/hubs/collaboration");

app.Run();
