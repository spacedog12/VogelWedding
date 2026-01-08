using BlazorCurrentDevice;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Radzen;
using VogelWedding;
using VogelWedding.Interfaces;
using VogelWedding.Services;
using VogelWedding.ViewModels;

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
			AutoConnectRealtime = true,
		}
	)
);
builder.Services.AddScoped<SupabaseService>();
builder.Services.AddScoped<InformationSectionService>();
builder.Services.AddRadzenComponents();
builder.Services.AddMudServices();
builder.Services.AddBlazorCurrentDevice();

// Register the SupabasePhotosService
builder.Services.AddScoped<ISupabasePhotosService, SupabasePhotosService>();
builder.Services.AddTransient<PhotosViewModel>();

await builder.Build().RunAsync();
