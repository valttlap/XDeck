using BarRaider.SdTools;

using Newtonsoft.Json;

namespace XDeck.Models;

public class LightStatusSettings : BaseDatarefSettings
{
    [JsonProperty(PropertyName = "modeImage")]
    public bool ImageMode { get; set; } = false;

    [JsonProperty(PropertyName = "modeTitle")]
    public bool TitleMode { get; set; } = true;

    [JsonProperty(PropertyName = "modeTitleImage")]
    public bool TitleImageMode { get; set; } = false;

    [FilenameProperty]
    [JsonProperty(PropertyName = "image0")]
    public string? Image0 { get; set; }

    [FilenameProperty]
    [JsonProperty(PropertyName = "image1")]
    public string? Image1 { get; set; }

    [FilenameProperty]
    [JsonProperty(PropertyName = "image2")]
    public string? Image2 { get; set; }

    [JsonProperty(PropertyName = "title0")]
    public string? Title0 { get; set; } = "0";

    [JsonProperty(PropertyName = "title1")]
    public string? Title1 { get; set; } = "1";

    [JsonProperty(PropertyName = "title2")]
    public string? Title2 { get; set; } = "2";
}