using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Canvas.Windows.Forms.Host;
using Canvas.Windows.Forms;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<HostFileSystemHttpClient>();
builder.Services.AddScoped<IHostFileSystem>(sp => sp.GetRequiredService<HostFileSystemHttpClient>());

builder.Services.AddScoped<IHostFileUpload, HostFileUploadInterop>();

var host = builder.Build();

HostFileSystem.Current = new CombinedHostFileSystem(
    host.Services.GetRequiredService<IHostFileSystem>(),
    host.Services.GetRequiredService<IHostFileUpload>());

await host.RunAsync();
