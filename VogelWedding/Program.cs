using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Radzen;
using VogelWedding;
using VogelWedding.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<AccessService>();
builder.Services.AddScoped<AppSettings>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped(_ =>
	new Supabase.Client(
		builder.Configuration["Supabase:Url"],
		builder.Configuration["Supabase:AnonKey"],
		new Supabase.SupabaseOptions
		{
			AutoRefreshToken = true,
			AutoConnectRealtime = false,
		}
	)
);
builder.Services.AddScoped<SupabaseService>();
builder.Services.AddRadzenComponents();
builder.Services.AddMudServices();


await builder.Build().RunAsync();
