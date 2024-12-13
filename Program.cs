using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Trackor;
using Trackor.Features.Database;
using Trackor.Features.Pomodoro;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new PomodoroTimerService());
builder.Services.AddFluxor(o =>
{
    o.ScanAssemblies(typeof(Program).Assembly);
#if DEBUG
        o.UseReduxDevTools();
#endif
});

builder.Services.AddBesqlDbContextFactory<TrackorContext>(options => options.UseSqlite("Data Source=trackor.sqlite3"));
builder.Services.AddTrackorDb();
builder.Services.AddMudServices(config => config.ConfigureWithTrackorDefaults());

await builder.Build().RunAsync();
