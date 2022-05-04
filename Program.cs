using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SqliteWasmHelper;
using Trackor;
using Trackor.Features.Database;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddFluxor(o =>
{
    o.ScanAssemblies(typeof(Program).Assembly);
    o.UseReduxDevTools();
});
builder.Services.AddSqliteWasmDbContextFactory<TrackorContext>(options => options.UseSqlite("Data Source=trackor.sqlite3", x => x.MigrationsAssembly(nameof(Trackor))));

await builder.Build().RunAsync();
