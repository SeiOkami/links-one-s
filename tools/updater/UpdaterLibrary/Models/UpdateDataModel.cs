namespace UpdaterLibrary.Models;

public class UpdateDataModel
{
    public MetadataModel Metadata { get; set; } = new();
    public List<TagModel> Tags { get; set; } = new(); 
    public List<ChannelModel> Channels { get; set; } = new();
}
