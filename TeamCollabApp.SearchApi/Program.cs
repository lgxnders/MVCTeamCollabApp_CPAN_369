using Microsoft.EntityFrameworkCore;
using TeamCollabApp.SearchApi.Data;
using TeamCollabApp.SearchApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
