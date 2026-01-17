using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;
using ClientApp.Services;
using Shared.DTOs;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read API base URL from configuration (wwwroot/appsettings.json); fallback if missing
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7058/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });

// Register API services
builder.Services.AddScoped<ProductApiService>();
builder.Services.AddScoped<CategoryApiService>();

// Also register via generic interface
builder.Services.AddScoped<IApiService<ProductDto>>(sp => sp.GetRequiredService<ProductApiService>());
builder.Services.AddScoped<IApiService<CategoryDto>>(sp => sp.GetRequiredService<CategoryApiService>());

await builder.Build().RunAsync();
