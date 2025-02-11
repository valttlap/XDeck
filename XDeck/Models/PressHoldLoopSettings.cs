using Newtonsoft.Json;

namespace XDeck.Models;

public class PressHoldLoopSettings : BaseCommandSettings
{
    [JsonProperty(PropertyName = "waitTime")]
    public int WaitTime { get; set; } = 500;

    [JsonProperty(PropertyName = "loopTime")]
    public int LoopTime { get; set; } = 100;
}