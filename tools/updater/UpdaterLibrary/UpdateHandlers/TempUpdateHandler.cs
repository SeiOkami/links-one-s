using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdaterLibrary.Settings;
using UpdaterLibrary.SiteUpdater;
using Telegram.Bot.Types.Enums;

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
