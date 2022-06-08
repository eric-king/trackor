## Trackor

### A developer-oriented task tracker written in Blazor WebAssembly

#### What Trackor is:
Trackor is a side-project I'm using to experiment with a few technologies
that interest me:
- [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
- PWA ([Progressive Web Application](https://en.wikipedia.org/wiki/Progressive_web_application))
- [EF Core](https://docs.microsoft.com/en-us/ef/core/) backed by a SQLite
 database, _in the browser cache_, with the help of
 [SqliteWasmHelper](https://github.com/JeremyLikness/SqliteWasmHelper)
- Application state using the Flux pattern with the [Fluxor](https://github.com/mrpmorris/Fluxor) library
- [MudBlazor](https://www.mudblazor.com/) components and UI styling
- [Azure Static Web Apps](https://azure.microsoft.com/en-us/services/app-service/static/)


It is a web application that has no back-end services - everything is accomplished
locally in the browser. The project is deployed to: [https://app.trackor.dev](https://app.trackor.dev)

#### What Trackor is not:
Trackor is not a product. I don't have the time or resources to maintain
a full-blown project with updates and support indefinitely. Feel free to
use the code here as inspiration for your own project if you'd like.

#### Screenshots

##### Landing Page
<img src="screenshots/landing_page.png" alt="Landing Page" width="600">

##### Activity Log
<img src="screenshots/activity_log.png" alt="Activity Log" width="600">

##### Task List
<img src="screenshots/task_list.png" alt="Task List" width="600">

##### Code Snippets
<img src="screenshots/code_snippets.png" alt="Code Snippets" width="600">

##### Pomodoro Timer
<img src="screenshots/pomodoro.png" alt="Pomodoro Timer" width="600">

##### Database Download / Restore
<img src="screenshots/database.png" alt="Database Download / Restore" width="600">