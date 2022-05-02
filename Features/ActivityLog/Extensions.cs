using System.Text;
using Trackor.Features.Categories;
using Trackor.Features.Projects;

namespace Trackor.Features.ActivityLog;

public static class Extensions
{
    public static string ToClipboardString(this ActivityLogItem item, Category[] categories, Project[] projects) 
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{item.Date.ToShortDateString()}: ");

        if (item.Duration > TimeSpan.Zero)
        {
            stringBuilder.Append($"({item.Duration.ToString(@"hh\:mm")}) ");
        }

        if (item.CategoryId is not null)
        {
            stringBuilder.Append($"{categories.Single(x => x.Id == item.CategoryId).Title} - ");
        }

        if (item.ProjectId is not null)
        {
            stringBuilder.Append($"{projects.Single(x => x.Id == item.ProjectId).Title} - ");
        }

        stringBuilder.Append($"{item.Title.Trim()}");
        return stringBuilder.ToString();
    }
}

