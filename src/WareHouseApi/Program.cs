using Microsoft.Data.Sqlite;
using WareHouseApi.Infrastructure;
using WareHouseApi.WarehouseProduct;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

EventTypeResolver.RegisterEvents<Program>();

builder.Services.AddScoped(typeof(Repository<>), typeof(Repository<>));
builder.Services.AddScoped<IEventStore, SqliteEventStore>();

var connectionString = builder.Configuration.GetConnectionString("Sqlite");
builder.Services.AddScoped(serviceProvider => new SqliteConnection(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Warehouse Api");

app.MapWarehouseProductEndpoints();

DbInitializer.Init(connectionString);

DbInitializer.AddEntityEvent("WarehouseProduct", "ProductReceived", connectionString);
DbInitializer.AddEntityEvent("WarehouseProduct", "ProductShipped", connectionString);
DbInitializer.AddEntityEvent("WarehouseProduct", "InventoryAdjusted", connectionString);

app.Run();
