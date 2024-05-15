using UpdaterLibrary.SiteUpdater;
using Telegram.Bot.Types.Enums;
using UpdaterLibrary.Settings;

namespace UpdaterLibrary.UpdateHandlers;

internal class ChannelInfoUpdateHandler : IUpdateHandler
{
    public async Task UpdateAsync(IUpdater updater)
    {
        foreach (var channel in updater.Data.Channels)
        {
            if (!updater.ChannelUpdateRequired(channel))
                continue;

            var info = await updater.GetChatAsync(channel);
            if (info is null)
                continue;

            bool modified = false;

            var hasFirstOrLastName = info.FirstName is not null || info.LastName is not null;

            if (info.Id != 0 && info.Id != channel.ID)
            {
                channel.ID = info.Id;
                modified = true;
            }

            var usename = info.Username ?? "";
            if (!String.IsNullOrEmpty(usename) && usename != channel.UserName)
            {
                channel.UserName = usename;
                modified = true;
            }

            var description = info.Description ?? "";
            if (description != channel.Description)
            {
                channel.Description = description;
                modified = true;
            }

            string title;
            if (info.Title is null && hasFirstOrLastName)
                title = $"{info.FirstName} {info.LastName ?? ""}";
            else
                title = info.Title ?? "";

            if (title != channel.Name)
            {
                channel.Name = title;
                modified = true;
            }

            var isChannel = info.Type == ChatType.Channel;
            if (channel.IsChannel != isChannel)
            {
                channel.IsChannel = isChannel;
                modified = true;
            }

            if (modified)
            {
                updater.AddToLog("Актуализирована информация о канале", channel);
            }

            var isBot = !isChannel 
                && channel.UserName.EndsWith("bot", true, null) 
                && info.Type == ChatType.Private
                && !hasFirstOrLastName;

            var isUser = !isChannel && hasFirstOrLastName;
            var isChat = !isChannel && !isBot && !isUser;
            
            updater.UpdateChannelTag(channel, SettingsManager.TAG_NAME_CHANNEL, isChannel);
            updater.UpdateChannelTag(channel, SettingsManager.TAG_NAME_CHAT   , isChat);
            updater.UpdateChannelTag(channel, SettingsManager.TAG_NAME_BOT    , isBot);
            updater.UpdateChannelTag(channel, SettingsManager.TAG_NAME_USER   , isUser);

        }
    }
}
