using System.Text.Json.Serialization;

namespace UpdaterLibrary.Models;

public class MetadataColumnModel
{
    [JsonRequired]
    public string Name { get; set; } = string.Empty;

    [JsonRequired]
    public string Presentation { get; set; } = string.Empty;
}
