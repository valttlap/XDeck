using BarRaider.SdTools;
using Newtonsoft.Json;
using XPlaneConnector;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.freqlistener")]
    public class FreqListenerAction : KeypadBase
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
            public string Dataref { get; set; }

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; }

            [JsonProperty(PropertyName = "unit")]
            public string Units { get; set; }
        }
        #endregion

        protected readonly PluginSettings settings;

        private readonly XConnector _connector;
        private string _currentDataref = null;

        public FreqListenerAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0) // Called the first time you drop a new action into the Stream Deck
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            _connector = XConnector.Instance;
            SubscribeDataref();
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Destructor called");
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
            try { Tools.AutoPopulateSettings(settings, payload.Settings); }
            catch { }
            SubscribeDataref();
        }

        private void SubscribeDataref()
        {
            if (_currentDataref == settings.Dataref) return;
            if (_currentDataref != null)
            {
                _connector.Connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            _currentDataref = settings.Dataref;

            var dataref = new DataRefElement
            {
                DataRef = settings.Dataref,
                Units = "Unknown",
                Description = "Userdefined datared",
                Frequency = settings.Frequency
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Subscribing dataref: {settings.Dataref}");
            _connector.Connector.Subscribe(dataref, dataref.Frequency, async (element, val) =>
            {
                val /= 1000;
                var stringVal = val.ToString("F3");
                await Connection.SetTitleAsync($"{stringVal}");
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Subscribed dataref: {settings.Dataref}");
            });
        }
    }
}
