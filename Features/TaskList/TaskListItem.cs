namespace Trackor.Features.TaskList;

public class TaskListItem
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public int? ProjectId { get; set; }
    public string Narrative { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
    public DateOnly? Due { get; set; }

    public string DueDisplayText => Due.HasValue ? $"Due: {Due}" : string.Empty;
    public bool DueToday => Status != TaskListItemStatus.Done && Due.HasValue && Due.Value == DateOnly.FromDateTime(DateTime.Today);
    public bool Overdue => Status != TaskListItemStatus.Done && Due.HasValue && Due.Value < DateOnly.FromDateTime(DateTime.Today);
}

public class TaskListItemStatus 
{
    public const int ToDo = 0;
    public const int InProgress = 1;
    public const int Done = 2;
}
