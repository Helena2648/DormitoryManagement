using DormitoryManagement.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DormitoryDbContext>(options =>
    options.UseMySQL(connectionString!));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<DormitoryDbContext>();

    try
    {
        var canConnect = db.Database.CanConnect();
        Console.WriteLine(
            $"MySQL connection successful: {canConnect}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MySQL connection failed: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();