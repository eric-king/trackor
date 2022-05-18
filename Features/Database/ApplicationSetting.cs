namespace Trackor.Features.Database;

public record ApplicationSetting
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}
