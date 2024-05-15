using TelegramTypes = Telegram.Bot.Types;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.SiteUpdater;

public interface IUpdater
{
    public UpdateDataModel Data { get; set; }
    public Dictionary<string, LogEventModel> Log { get; set; }
    public UpdateParametrs Parametrs { get; set; }
    public Task UpdateAsync();
    public bool ChannelUpdateRequired(ChannelModel channel);
    public void UpdateChannelTag(ChannelModel channel, string TagName, bool Add);
    public void AddLogHandler(Action<string, object, LogEventType> handler);
    public void AddToLog(string message, object obj, LogEventType type = LogEventType.Change);
    public string LogText(LogEventType? filterType = null);
    public Task<int?> GetMemberCountAsync(ChannelModel channel);
    public Task<TelegramTypes.Chat?> GetChatAsync(ChannelModel channel);
}
