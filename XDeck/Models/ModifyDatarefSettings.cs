using Newtonsoft.Json;

namespace XDeck.Models;

public class ModifyDatarefSettings : BaseDatarefSettings
{
    [JsonProperty(PropertyName = "modeIncrease")]
    public bool IncreaseDatarefMode { get; set; } = true;

    [JsonProperty(PropertyName = "modeDecrease")]
    public bool DecreaseDatarefMode { get; set; } = false;

    [JsonProperty(PropertyName = "modeSet")]
    public bool SetDatarefMode { get; set; } = false;

    [JsonProperty(PropertyName = "maxRefVal")]
    public string MaxRefValue { get; set; } = "1";
    [JsonProperty(PropertyName = "minRefVal")]
    public string MinRefValue { get; set; } = "0";

    [JsonProperty(PropertyName = "setRefVal")]
    public string SetRefValue { get; set; } = "1";
}