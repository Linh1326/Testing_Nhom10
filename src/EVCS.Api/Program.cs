using System.Text.Json.Serialization;
using EVCS.Api.Middlewares;
using EVCS.Application;
using EVCS.Infrastructure;
using EVCS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("EVCS API đang chạy."));

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.EnsureCreated();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể khởi tạo cơ sở dữ liệu. Vui lòng kiểm tra lại chuỗi kết nối và quyền truy cập SQL Server.");
    }
}

app.Run();
