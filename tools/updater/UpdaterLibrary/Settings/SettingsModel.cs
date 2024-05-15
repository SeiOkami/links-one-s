namespace UpdaterLibrary.Settings;
public class SettingsModel
{
    public string TelegramBotToken { get; set; } = string.Empty;
    public string PathToSite { get; set; } = string.Empty;
    public int DelayRequestsApi { get; set; }
    public List<string> Subscribers { get; set; } = new();
}
