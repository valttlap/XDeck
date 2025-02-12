using System.Drawing;

using Newtonsoft.Json;

namespace XDeck.Models;

public class MultiListenerSettings : BaseDatarefSettings
{
    private string? _settingsJson;

    [JsonProperty(PropertyName = "settingsJson")]
    public string? SettingsJson
    {
        get => _settingsJson;
        set
        {
            _settingsJson = value;
            if (!string.IsNullOrEmpty(_settingsJson))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<Dictionary<int, DataRef>>(_settingsJson) ?? [];
                }
                catch (JsonException)
                {
                    Settings = [];
                }
            }
            else
            {
                Settings = [];
            }
        }
    }

    [JsonIgnore]
    public Dictionary<int, DataRef> Settings { get; set; } = [];
}

public class DataRef
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("imagePath")]
    public string? ImagePath { get; set; }
    [JsonIgnore]
    public Image? Image { get; set; }
}