using UpdaterLibrary.SiteUpdater;

namespace UpdaterLibrary.UpdateHandlers;

public interface IUpdateHandler
{
    public Task UpdateAsync(IUpdater updater);
}
