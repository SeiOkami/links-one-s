using UpdaterLibrary.Models;
using UpdaterLibrary.UpdateHandlers;

namespace UpdaterLibrary.Tests;

[TestClass]
public class IrrelevantTagsUpdateHandlerTest
{
    [TestMethod]
    public async Task TestUpdateAsync()
    {
        var updater = new FakeUpdater();
        updater.Data.Tags.Add(new TagModel(){
                Name = "test",
        });

        updater.Data.Channels.Add(new ChannelModel(){
                Tags = ["test"]
        });

        var handler = new IrrelevantTagsUpdateHandler();
        await handler.UpdateAsync(updater);
        
        

    }
}