using BarRaider.SdTools;
using Newtonsoft.Json;
using XDeck.Backend;
using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.presshold")]
    public class PressHoldAction : KeypadBase
    {
        #region Settings

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Command = "sim/none/none"
                };

                return instance;
            }

            [JsonProperty(PropertyName = "xplaneCommand")]
            public string Command { get; set; } = "sim/none/none";
        }
        #endregion

        protected readonly PluginSettings? settings;

        private readonly XConnector _connector;

        private CancellationTokenSource? _cancelToken;
        public PressHoldAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (settings == null) return;
            var command = new XPlaneCommand(settings.Command, "Userdefined command");
            _cancelToken = _connector.StartCommand(command);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Started command: {settings.Command}");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (_cancelToken == null) return;
            _cancelToken.Cancel();
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Cancelled command: {settings?.Command}");
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
        }
    }
}
