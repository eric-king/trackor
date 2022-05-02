namespace Trackor.Features.LandingPage;

public record LandingPageItem
{
    public string ImageUrl { get; init; }
    public string HeaderText { get; init; }
    public string BodyText { get; set; }
    public ItemLink[] Links { get; init; }
}

public record ItemLink
{
    public string Url { get; init; }
    public string Text { get; init; }
}