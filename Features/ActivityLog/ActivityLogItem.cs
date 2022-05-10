namespace Trackor.Features.ActivityLog;

public class ActivityLogItem
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public int? ProjectId { get; set; }
    public string Title { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Archived { get; set; }
}
