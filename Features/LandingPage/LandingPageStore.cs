using Fluxor;

namespace Trackor.Features.LandingPage;

public record LandingPageState
{
    public LandingPageItem[] Items { get; init; }
}

public class LandingPageFeature : Feature<LandingPageState>
{
    public override string GetName() => "LandingPage";

    protected override LandingPageState GetInitialState()
    {
        return new LandingPageState
        {
            Items = new LandingPageItem[]
            {
                new LandingPageItem
                {
                    HeaderText = "Activity Log",
                    BodyText = "Log your daily activities",
                    ImageUrl = "images/activitylog.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Activity Log",
                            Url = "/activityLog"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Task List",
                    BodyText = "Keep track of your tasks",
                    ImageUrl = "images/stickies.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Task List",
                            Url = "/tasklist"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Code Snippets",
                    BodyText = "Store useful pieces of code for future reference",
                    ImageUrl = "images/snippets.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Code Snippets",
                            Url = "/snippets"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Pomodoro",
                    BodyText = "Direct your attention with manageable chunks of time",
                    ImageUrl = "images/countdown_timer.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Pomodoro",
                            Url = "/pomodoro"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Organize",
                    BodyText = "Organize your Trackor content",
                    ImageUrl = "images/organize.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Categories",
                            Url = "/categories"
                        },
                        new ItemLink
                        {
                            Text = "Projects",
                            Url = "/projects"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Weather",
                    BodyText = "Get current weather conditions for your location",
                    ImageUrl = "images/sky_clouds.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Weather",
                            Url = "/weather"
                        }
                    }
                },
                new LandingPageItem
                {
                    HeaderText = "Database",
                    BodyText = "Backup or Restore the Trackor database",
                    ImageUrl = "images/database.jpg",
                    Links = new ItemLink[]
                    {
                        new ItemLink
                        {
                            Text = "Database",
                            Url = "/database"
                        }
                    }
                }
            }
        };
    }
}