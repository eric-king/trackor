﻿namespace Trackor.Features.ActivityLog.Model;

public record ActivityLogItem
{
    public int Id { get; init; }
    public int? CategoryId { get; init; }
    public int? ProjectId { get; init; }
    public string Title { get; init; }
    public DateOnly Date { get; init; }
    public TimeSpan Duration { get; init; }
}
