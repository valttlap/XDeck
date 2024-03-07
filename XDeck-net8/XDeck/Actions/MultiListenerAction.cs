using System.Drawing;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XDeck.Backend;
using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.multilistener")]
public class MultiListenerAction : KeypadBase
{
    #region Settings
    protected class PluginSettings
    {
        private string? _settingsJson;
        [JsonProperty(PropertyName = "dataref")]
        public string Dataref { get; set; } = "sim/none/none";

        [JsonProperty(PropertyName = "pollFreq")]
        public int Frequency { get; set; } = 5;

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

        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new();
            return instance;
        }
    }

    protected class DataRef
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("imagePath")]
        public string? ImagePath { get; set; }
        [JsonIgnore]
        public Image? Image { get; set; }
    }
    #endregion
    private PluginSettings? _settings;
    private readonly object _imageLock = new();
    private readonly XConnector _connector;
    private string? _currentDataref;
    private int? _currentValue = 0;

    public MultiListenerAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0) // Called the first time you drop a new action into the Stream Deck
        {
            _settings = PluginSettings.CreateDefaultSettings();
        }
        else
        {
            _settings = payload.Settings.ToObject<PluginSettings>();
        }
        _connector = XConnector.Instance;
        if (_settings == null || _settings.Settings == null || _settings.Settings.Count == 0) return;
        InitializeSettings();
        SubscribeDataref();
        SaveSettings();

    }
    public override void Dispose()
    {
        if (_currentDataref != null)
        {
            _connector.Unsubscribe(_currentDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
        }
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
        GC.SuppressFinalize(this);
    }

    public override void KeyPressed(KeyPayload payload)
    {
    }

    public override void KeyReleased(KeyPayload payload)
    {
    }

    public override void OnTick()
    {
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        try { Tools.AutoPopulateSettings(_settings, payload.Settings); }
        catch { }
        if (_settings == null || _settings.Settings == null || _settings.Settings.Count == 0) return;
        InitializeSettings();
        SubscribeDataref();
        SaveSettings();
    }

    private void SubscribeDataref()
    {
        if (_settings == null) return;
        if (_currentDataref == _settings.Dataref) return;
        if (_currentDataref != null)
        {
            _connector.Unsubscribe(_currentDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
        }
        _currentDataref = _settings.Dataref;

        var dataref = new DataRefElement
        {
            DataRef = _settings.Dataref,
            Units = "Unknown",
            Description = "Userdefined datared",
            Frequency = _settings.Frequency
        };
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {_settings.Dataref}");
        _connector.Subscribe(dataref, async (element, val) =>
        {
            _currentValue = (int)val;
            await SetImageTitleAsync();
        });
    }

    private async Task SetImageTitleAsync()
    {
        if (_settings == null || _settings.Settings.Count == 0)
        {
            await SetDefaultsAsync();
            return;
        }

        if (!_settings.Settings.TryGetValue(_currentValue ?? 0, out var imageTitle))
        {
            await SetDefaultsAsync();
            return;
        }

        var tasks = new List<Task>();

        if (imageTitle.Image != null)
        {
            tasks.Add(Connection.SetImageAsync(imageTitle.Image));
        }
        else
        {
            tasks.Add(Connection.SetDefaultImageAsync());
        }

        if (!string.IsNullOrEmpty(imageTitle.Title))
        {
            tasks.Add(Connection.SetTitleAsync(imageTitle.Title));
        }
        else
        {
            tasks.Add(Connection.SetTitleAsync(""));
        }

        if (tasks.Count == 0)
        {
            await SetDefaultsAsync();
            return;
        }

        await Task.WhenAll(tasks);
    }

    // Helper method to set default image and title concurrently.
    private async Task SetDefaultsAsync()
    {
        await Task.WhenAll(Connection.SetDefaultImageAsync(), Connection.SetTitleAsync("Unknown"));
    }

    protected virtual void InitializeSettings()
    {
        PrefetchImages();
    }

    private void PrefetchImages()
    {
        if (_settings == null) return;
        if (_settings.Settings == null || _settings.Settings.Count == 0) return;
        lock (_imageLock)
        {
            foreach (var dataref in _settings.Settings)
            {
                dataref.Value.Image = LoadImage(dataref.Value.ImagePath);
            }
        }
    }

    private static Image? LoadImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) return null;
        return Image.FromFile(imagePath);
    }

    private void SaveSettings()
    {
        if (_settings == null) return;
        Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }
}
