using System.Data;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XDeck.Backend;
using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.aplistener")]
public class APListenerAction : KeypadBase
{
    #region Settings

    protected class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new();
            return instance;
        }

        [JsonProperty(PropertyName = "dataref")]
        public string Dataref { get; set; } = "sim/none/none";

        [JsonProperty(PropertyName = "pollFreq")]
        public int Frequency { get; set; } = 5;

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "Unknown";

        [JsonProperty(PropertyName = "dashedDataref")]
        public string DashedDataref { get; set; } = "sim/none/none";

        [JsonProperty(PropertyName = "managedDataref")]
        public string ManagedDataref { get; set; } = "sim/none/none";

        [JsonProperty(PropertyName = "isMachDataref")]
        public string IsMachDataref { get; set; } = "sim/none/none";
    }

    private readonly PluginSettings? _settings;
    private readonly XConnector _connector;
    private delegate void DatarefChanged(string value);
    private event DatarefChanged? OnDatarefChanged;
    private float? _currentValue = null;
    private bool _isDashed = false;
    private bool _isManaged = false;
    private bool _isMach = false;
    private string? _currentDataref = null;
    private string? _currentDashedDataref = null;
    private string? _currentManagedDataref = null;
    private string? _currentIsMachDataref = null;


    #endregion
    public APListenerAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
        if (_settings == null) return;
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
        if (_currentDashedDataref != null)
        {
            _connector.Unsubscribe(_currentDashedDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDashedDataref}");
        }
        if (_currentManagedDataref != null)
        {
            _connector.Unsubscribe(_currentManagedDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentManagedDataref}");
        }
        if (_currentIsMachDataref != null)
        {
            _connector.Unsubscribe(_currentIsMachDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentIsMachDataref}");
        }
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
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
        Tools.AutoPopulateSettings(_settings, payload.Settings);
        SubscribeDataref();
        SaveSettings();
    }

    private void SubscribeDataref()
    {
        if (_settings == null) return;

        // Refactor common logic into a method
        void SubscribeIfChanged(ref string? currentDataRef, string newDataRef, Action<DataRefElement, float> onSubscribeAction)
        {
            if (currentDataRef == newDataRef) return;

            if (currentDataRef != null)
            {
                _connector.Unsubscribe(currentDataRef);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {currentDataRef}");
            }

            currentDataRef = newDataRef;
            if (currentDataRef == null) return;

            var dataRefElement = new DataRefElement
            {
                DataRef = currentDataRef,
                Units = "Unknown",
                Description = "Userdefined dataref", // Note the typo correction
                Frequency = _settings.Frequency
            };

            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {currentDataRef}");
            _connector.Subscribe(dataRefElement, onSubscribeAction);
        }

        // Simplify the calls with the new method
        SubscribeIfChanged(ref _currentDataref, _settings.Dataref, async (element, val) => { _currentValue = val; SetDisplayValue(); });
        SubscribeIfChanged(ref _currentDashedDataref, _settings.DashedDataref, async (element, val) => { _isDashed = val == 1; SetDisplayValue(); });
        SubscribeIfChanged(ref _currentManagedDataref, _settings.ManagedDataref, async (element, val) => { _isManaged = val == 1; SetDisplayValue(); });
        SubscribeIfChanged(ref _currentIsMachDataref, _settings.IsMachDataref, async (element, val) => { _isMach = val == 1; SetDisplayValue(); });
    }

    private async Task SetDisplayValue()
    {
        await Connection.SetTitleAsync(GetDisplayValue());
    }

    private string GetDisplayValue()
    {
        if (_settings == null || !_currentValue.HasValue) return "INVALID";

        string baseResult = GetBaseDisplayValue();
        if (_isManaged)
        {
            baseResult += " •";
        }

        string prefix = GetPrefix();
        return $"{prefix}\n{baseResult}";
    }

    private string GetBaseDisplayValue()
    {
        switch (_settings.Type)
        {
            case "SPEED":
                return _isDashed ? (_isMach ? "-.--" : "---") : (_isMach ? Math.Round(_currentValue.Value, 2).ToString() : ((int)Math.Round(_currentValue.Value)).ToString());
            case "HEADING":
                return _isDashed ? "---" : ((int)Math.Round(_currentValue.Value)).ToString();
            case "ALTITUDE":
                return ((int)Math.Round(_currentValue.Value)).ToString();
            case "VERTICALSPEED":
                if (_isDashed) return "-----";
                var value = Math.Abs((int)Math.Round(_currentValue.Value)).ToString();
                return _currentValue.Value < 0 ? "-" + value : "+" + value;
            default:
                return "INVALID";
        }
    }

    private string GetPrefix()
    {
        switch (_settings.Type)
        {
            case "SPEED":
                return _isMach ? "MACH" : "SPD";
            case "HEADING":
                return "HDG";
            case "ALTITUDE":
                return "ALT";
            case "VERTICALSPEED":
                return "V/S";
            default:
                return ""; // Default case should never hit due to earlier validation
        }
    }

    private void SaveSettings()
    {
        if (_settings == null) return;
        Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }
}
