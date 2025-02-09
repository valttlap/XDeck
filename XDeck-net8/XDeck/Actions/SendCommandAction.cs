using BarRaider.SdTools;

using Newtonsoft.Json;

using XDeck.Backend;

using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.sendcommand")]
    public class SendCommandAction(SDConnection connection, InitialPayload payload) : KeypadBase(connection, payload)
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

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
            GC.SuppressFinalize(this);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (_settings == null) return;
            var command = new XPlaneCommand(_settings.Command, "Userdefined command");
            _connector.SendCommand(command);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Sent command: {_settings.Command}");
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
        }
    }
}