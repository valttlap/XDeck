using BarRaider.SdTools;

using Newtonsoft.Json;

using XDeck.Backend;

using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.presshold")]
    public class PressHoldAction(SDConnection connection, InitialPayload payload) : KeypadBase(connection, payload)
    {
        #region Settings

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new()

                {
                    Command = "sim/none/none"
                };

                return instance;
            }

            [JsonProperty(PropertyName = "xplaneCommand")]
            public string Command { get; set; } = "sim/none/none";
        }
        #endregion

        protected readonly PluginSettings? _settings = payload.Settings == null || payload.Settings.Count == 0
                ? PluginSettings.CreateDefaultSettings()
                : payload.Settings.ToObject<PluginSettings>();

        private readonly XConnector _connector = XConnector.Instance;

        private CancellationTokenSource? _cancelToken;

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
            GC.SuppressFinalize(this);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (_settings == null) return;
            var command = new XPlaneCommand(_settings.Command, "Userdefined command");
            _cancelToken = _connector.StartCommand(command);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Started command: {_settings.Command}");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (_cancelToken == null) return;
            _cancelToken.Cancel();
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Cancelled command: {_settings?.Command}");
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
        }
    }
}