using Newtonsoft.Json;

namespace XDeck.Models;

public abstract class BaseDatarefSettings
{
    [JsonProperty("dataref")]
    public string Dataref { get; set; } = "sim/none/none";

    [JsonProperty("pollFreq")]
    public int Frequency { get; set; } = 5;
}