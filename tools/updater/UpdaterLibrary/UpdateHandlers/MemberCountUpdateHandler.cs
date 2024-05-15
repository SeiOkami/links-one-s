using UpdaterLibrary.Settings;
using UpdaterLibrary.SiteUpdater;

namespace UpdaterLibrary.UpdateHandlers;

internal class MemberCountUpdateHandler : IUpdateHandler
{
    const int MEMBER_COUNT_TAG_BEGIN = 500;

    public async Task UpdateAsync(IUpdater updater)
    {
        foreach (var channel in updater.Data.Channels)
        {
            if (!updater.ChannelUpdateRequired(channel))
                continue;
            
            var count = await updater.GetMemberCountAsync(channel);
            if (count is null)
                continue;

            //TODO: вынести типы, которые сейчас в тегах.
            //Сейчас считаем, что если 2 участника чата, то это чат с ботом или пользователем
            //  и для него не нужно ставить тег "Малыши"
            updater.UpdateChannelTag(channel,
                SettingsManager.TAG_NAME_BEGIN,
                count <= MEMBER_COUNT_TAG_BEGIN && count > 2);
            
            count = count / 1000;
            if (count != channel.MemberCount)
            {
                channel.MemberCount = (int)count;
                updater.AddToLog("Актуализировано число подписчиков", channel);
            }

        }
    }
}
