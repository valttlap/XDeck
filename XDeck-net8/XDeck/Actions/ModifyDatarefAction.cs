using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XDeck.Backend;
using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.modifydataref")]
    public class ModifyDatarefAction : KeypadBase
    {
        #region Settings
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                // Default settings are applied here, assuming they meet the initial business logic requirements.
                return new PluginSettings
                {
                    Dataref = "sim/none/none",
                    Frequency = "55",
                    IncreaseDatarefMode = true, // Ensure default settings comply with the business rules
                    // DecreaseDatarefMode and SetDatarefMode are false by default
                    MaxRefValue = "1",
                    MinRefValue = "0",
                    SetRefValue = "1"
                    // MinRefValue and SetRefValue are null by default, which is acceptable since IncreaseDatarefMode is true
                };
            }

            [JsonProperty(PropertyName = "dataref")]
            public string Dataref { get; set; } = "sim/none/none";
            [JsonProperty(PropertyName = "pollFreq")]
            public string Frequency { get; set; } = "5";

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
        #endregion

        protected readonly PluginSettings? _settings;
        private readonly XConnector _connector;
        private string? _currentDataref;
        private int _currentValue = 0;


        public ModifyDatarefAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (_settings == null) return;
            if (_settings.IncreaseDatarefMode)
            {
                HandleIncreaseMode();
            }
            else if (_settings.DecreaseDatarefMode)
            {
                HandleDecreaseMode();
            }
            else if (_settings.SetDatarefMode)
            {
                HandleSetMode();
            }
        }

        private void HandleIncreaseMode()
        {
            if (_settings == null) return;
            if (_currentDataref is null) return;
            if (int.TryParse(_settings.MaxRefValue, out int maxVal) && _currentValue < maxVal)
            {
                _connector.SetDataRefValue(_currentDataref, _currentValue + 1);
            }
        }

        private void HandleDecreaseMode()
        {
            if (_settings == null) return;
            if (_currentDataref is null) return;
            if (int.TryParse(_settings.MinRefValue, out int minVal) && _currentValue > minVal)
            {
                _connector.SetDataRefValue(_currentDataref, _currentValue - 1);
            }
        }

        private void HandleSetMode()
        {
            if (_settings == null) return;
            if (_currentDataref is null) return;
            if (int.TryParse(_settings.SetRefValue, out int setVal))
            {
                _connector.SetDataRefValue(_currentDataref, setVal);
            }
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
            if (!int.TryParse(_settings.Frequency, out int freq)) return;
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
                Description = "User defined dataref",
                Frequency = freq
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {_settings.Dataref}");
            _connector.Subscribe(dataref, (element, val) =>
            {
                _currentValue = (int)val;
            });
        }

        private void SaveSettings()
        {
            if (_settings == null) return;
            Connection.SetSettingsAsync(JObject.FromObject(_settings));
        }
    }
}
