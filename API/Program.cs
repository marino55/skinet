using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // Kreira instancu kojom se pokreće naša aplikacija

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StoreContext>(opt => 
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline. (Midlewares)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Kreiranje scope-a, (Services.AddDbContext je isto scope-an kao i naš repository Services.AddScoped kojeg smo dependency injectali, scopeani su na http request)
// Koristimo using jer CreateScope() koristi IService interface, te IService interface koristi IDisposable() interface (završava scope lifetime), zato kad kod unutar tog servisa završi servis ce se zatvoriti
using var scoped = app.Services.CreateScope(); // Ovdje možemo pristupiti tom servisu unutar program klase gdje nemamo pravo pristupa injectati taj servis
var services = scoped.ServiceProvider;
var context = services.GetRequiredService<StoreContext>();
var logger = services.GetRequiredService<ILogger<Program>>();
try
{
    await context.Database.MigrateAsync(); // Asinkrono penda sve migracije kontekstu baze, i kreira bazu ako ne postoji
    await StoreContextSeed.SeedAsync(context); // Popunjavanje tablica podacima ako ih nema
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured durning migration");
}

app.Run();
