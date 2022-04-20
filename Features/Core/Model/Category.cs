namespace Trackor.Features.Core.Model;

public record Category
{
    public int Id { get; init; }
    public string Title { get; init; }
    public bool Active { get; init; }
}
