using IsakChat.Server.Data;
using IsakChat.Server.Endpoints;
using IsakChat.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<SessionAuth>();

builder.Services.AddCors(options =>
{
    // Skolski projekat, jedan server za jednog/nekoliko klijenata na istoj mrezi/masini - CORS ne mora da bude strog
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Napravi bazu i wwwroot/uploads folder ako ne postoje (student ne mora rucno da radi migracije)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads"));

app.UseCors();
app.UseStaticFiles(); // servira wwwroot/uploads/... kao /uploads/...

app.MapGet("/", () => "QuickChat server radi. 📡");

app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapMessagesEndpoints();

app.Run();
