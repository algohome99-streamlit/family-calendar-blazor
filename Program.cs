using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using FamilyCalendar.Blazor;
using FamilyCalendar.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// LocalStorage for auth state
builder.Services.AddBlazoredLocalStorage();

// Services
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
