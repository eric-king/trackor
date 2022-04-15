namespace Trackor.Features.Activity.Model;

public record Project
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public bool Active { get; init; }
}