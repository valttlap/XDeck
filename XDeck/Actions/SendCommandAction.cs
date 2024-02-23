using BarRaider.SdTools;
using Newtonsoft.Json;
using XPlaneConnector;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.sendcommand")]
    public class SendCommandAction : KeypadBase
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
            public string Command { get; set; }
        }
        #endregion

        protected readonly PluginSettings settings;

        private readonly XConnector _connector;
        public SendCommandAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            var command = new XPlaneCommand(settings.Command, "Userdefined command");
            _connector.SendCommand(command);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Sent command: {settings.Command}");
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
        }
    }
}
