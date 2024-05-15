using System.Text.Json.Serialization;

namespace UpdaterLibrary.Models;

public class TagModel
{
    [JsonRequired] 
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public override string ToString() => Name;

}
