using Telegram.Bot.Types;
using UpdaterLibrary.Models;
using UpdaterLibrary.SiteUpdater;

namespace UpdaterLibrary.Tests;
internal class FakeUpdater : IUpdater
{
    public UpdateDataModel Data { get; set; } = new();
    public Dictionary<string, LogEventModel> Log { get; set; } = new();

    public void AddToLog(string message, object obj, LogEventType type = LogEventType.Change)
    {
        throw new NotImplementedException();
    }

    public Task<Chat?> GetChatAsync(ChannelModel channel)
    {
        throw new NotImplementedException();
    }

    public Task<int?> GetMemberCountAsync(ChannelModel channel)
    {
        throw new NotImplementedException();
    }

    public string LogText()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync()
    {
        throw new NotImplementedException();
    }

    public void UpdateChannelTag(ChannelModel channel, string TagName, bool Add)
    {
        throw new NotImplementedException();
    }
}
