using Newtonsoft.Json;

namespace XDeck.Models;

public class ListenDatarefSettings : BaseDatarefSettings
{
    [JsonProperty("unit")]
    public string? Units { get; set; } = default;
}
