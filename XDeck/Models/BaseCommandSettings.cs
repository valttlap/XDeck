using Newtonsoft.Json;

namespace XDeck.Models;

public abstract class BaseCommandSettings
{
    [JsonProperty(PropertyName = "xplaneCommand")]
    public string Command { get; set; } = "sim/none/none";
}