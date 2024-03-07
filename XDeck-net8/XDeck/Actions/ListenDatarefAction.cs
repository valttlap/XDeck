using BarRaider.SdTools;
using Newtonsoft.Json;
using XDeck.Backend;
using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.listendataref")]
    public class ListenDatarefAction : KeypadBase
    {
        #region Settings
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Dataref = "sim/none/none"
                };

                return instance;
            }

            [JsonProperty(PropertyName = "dataref")]
            public string Dataref { get; set; } = "sim/none/none";

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; } = 5;

            [JsonProperty(PropertyName = "unit")]
            public string Units { get; set; } = "Unknown";
        }
        #endregion

        protected readonly PluginSettings? settings;

        private readonly XConnector _connector;
        private string? _currentDataref;

        public ListenDatarefAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0) // Called the first time you drop a new action into the Stream Deck
            {
                settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }
            _connector = XConnector.Instance;
            SubscribeDataref();
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
            try
            {
                Tools.AutoPopulateSettings(settings, payload.Settings);
            }
            catch { }
            SubscribeDataref();
        }

        private void SubscribeDataref()
        {
            if (settings == null) return;
            if (_currentDataref != null)
            {
                _connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            _currentDataref = settings.Dataref;

            var dataref = new DataRefElement
            {
                DataRef = settings.Dataref,
                Units = "Unknown",
                Description = "Userdefined datared",
                Frequency = settings.Frequency
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {settings.Dataref}");
            _connector.Subscribe(dataref, async (element, val) =>
            {
                await Connection.SetTitleAsync($"{val}  {settings.Units}");
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribed dataref: {settings.Dataref}");
            });
        }
    }
}
