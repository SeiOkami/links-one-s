using UpdaterLibrary.Settings;
using UpdaterLibrary.SiteUpdater;

namespace UpdaterLibrary.UpdateHandlers;
public class TempUpdateHandler : IUpdateHandler
{
    public Task UpdateAsync(IUpdater updater)
    {

        foreach (var channel in updater.Data.Channels)
        {
            channel.IsChannel = channel.Tags.Contains(SettingsManager.TAG_NAME_CHANNEL);
        }

        return Task.CompletedTask;
    }
}
