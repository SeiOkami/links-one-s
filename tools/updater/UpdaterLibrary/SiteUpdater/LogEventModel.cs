namespace UpdaterLibrary.SiteUpdater;

public class LogEventModel
{
    public List<object> Objects { get; set; } = new();
    public LogEventType Type { get; set; }
    public string Text { get; set; } = string.Empty;
}
