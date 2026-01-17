using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;
using ClientApp.Services;
using Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read API base URL from configuration (wwwroot/appsettings.json); fallback if missing
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7058/";

// Register ApiErrorHandler
builder.Services.AddTransient<ApiErrorHandler>();

// Register NotificationService
builder.Services.AddSingleton<NotificationService>();

// Register ProductState
builder.Services.AddSingleton<ProductState>();

// Configure a named HttpClient "Api" that uses the error handler and BaseAddress
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<ApiErrorHandler>();

// Register API services as typed clients resolved from IHttpClientFactory
builder.Services.AddScoped(sp => new ProductApiService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));
builder.Services.AddScoped(sp => new CategoryApiService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

// Also register via generic interface
builder.Services.AddScoped<IApiService<ProductDto>>(sp => sp.GetRequiredService<ProductApiService>());
builder.Services.AddScoped<IApiService<CategoryDto>>(sp => sp.GetRequiredService<CategoryApiService>());

await builder.Build().RunAsync();
