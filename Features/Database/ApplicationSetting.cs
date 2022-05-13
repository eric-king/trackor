namespace Trackor.Features.Database;

public record ApplicationSetting
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}

public class ApplicationSettingKeys 
{
    public const string DbVersion = "DbVersion";
    public const string IsDarkMode = "IsDarkMode";
    public const string PomodoroDuration = "PomodoroDuration";
}