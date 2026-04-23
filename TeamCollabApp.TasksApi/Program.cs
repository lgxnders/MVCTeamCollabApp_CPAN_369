using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TeamCollabApp.TasksApi.Data;
using TeamCollabApp.TasksApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    await db.Database.MigrateAsync();
    await new TasksSeeder(db).SeedAsync();
}

app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
