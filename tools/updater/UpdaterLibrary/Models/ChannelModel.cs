using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;
using UpdaterLibrary.Settings;

namespace UpdaterLibrary.Models;

public class ChannelModel
{

    [JsonIgnore]
    public string UserName { 
        get {
            return Url.Replace(SettingsManager.TELEGRAM_URL, "").Split('?')[0]; 
        }
        set {
            Url = SettingsManager.TELEGRAM_URL + value;
        }
    }

    public long ID { get; set; }
    [JsonRequired]
    public string Url { get; set; } = string.Empty;
    public bool IsChannel { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public DateTime Actual { get; set; }
    public int MemberCount { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<string> Tags { get; set; }

    public override string ToString() => UserName;

}
