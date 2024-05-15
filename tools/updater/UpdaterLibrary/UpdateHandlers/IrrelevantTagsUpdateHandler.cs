using UpdaterLibrary.SiteUpdater;

namespace UpdaterLibrary.UpdateHandlers;

public class IrrelevantTagsUpdateHandler : IUpdateHandler
{
    public Task UpdateAsync(IUpdater updater)
    {
        var data = updater.Data;

        foreach (var channel in data.Channels)
        {
            if (!updater.ChannelUpdateRequired(channel))
                continue;
            
            var irrelevantTags = channel.Tags.Where(
                    tagChannel => !data.Tags.Exists(t => t.Name == tagChannel)
                ).ToArray();

            foreach (var tag in irrelevantTags)
            {
                updater.UpdateChannelTag(channel, tag, false);
            }

        }

        return Task.CompletedTask;
    }
}
