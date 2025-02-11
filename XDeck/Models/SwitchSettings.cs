using BarRaider.SdTools;

using Newtonsoft.Json;

namespace XDeck.Models;

public class SwitchSettings : BaseDatarefSettings
{
   
    [JsonProperty(PropertyName = "command")]
    public string? Command { get; set; }

    [JsonProperty(PropertyName = "modeCommand")]
    public bool CommandMode { get; set; } = false;

    [JsonProperty(PropertyName = "modeDataref")]
    public bool DatarefMode { get; set; } = true;

    [JsonProperty(PropertyName = "modeImage")]
    public bool ImageMode { get; set; } = false;

    [JsonProperty(PropertyName = "modeTitle")]
    public bool TitleMode { get; set; } = true;

    [JsonProperty(PropertyName = "modeTitleImage")]
    public bool TitleImageMode { get; set; } = false;

    [FilenameProperty]
    [JsonProperty(PropertyName = "offImage")]
    public string? OffImage { get; set; }

    [FilenameProperty]
    [JsonProperty(PropertyName = "onImage")]
    public string? OnImage { get; set; }

    [JsonProperty(PropertyName = "offTitle")]
    public string OffTitle { get; set; } = "OFF";

    [JsonProperty(PropertyName = "onTitle")]
    public string OnTitle { get; set; } = "ON";
}