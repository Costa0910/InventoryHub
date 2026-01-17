using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Application.Services;
using ServerApp.Endpoints;
using ServerApp.Infrastructure.Persistence;
using ServerApp.Infrastructure.Repositories;
using ServerApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add in-memory caching
builder.Services.AddMemoryCache();

// Register DbContext and repositories
var connectionString = builder.Configuration.GetConnectionString("Inventory")
                       ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "inventory.db")}";

builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Register application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Use centralized global exception handler middleware
app.UseGlobalExceptionHandler();

// Seed initial data (skip during integration tests)
if (!app.Environment.IsEnvironment("Testing"))
{
    await DbSeeder.SeedAsync(app.Services);
}

// Enable CORS
app.UseCors("AllowFrontend");

// Map endpoint groups
app.MapProductEndpoints();
app.MapCategoryEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.RunAsync();

