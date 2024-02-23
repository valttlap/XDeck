using BarRaider.SdTools;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using XPlaneConnector;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.pressholdloop")]
    public class PressHoldLoopAction : KeypadBase
    {
        #region Settings

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Command = "sim/none/none",
                    WaitTime = 500,
                    LoopTime = 100
                };

                return instance;
            }

            [JsonProperty(PropertyName = "xplaneCommand")]
            public string Command { get; set; }

            [JsonProperty(PropertyName = "waitTime")]
            public int WaitTime { get; set; }

            [JsonProperty(PropertyName = "loopTime")]
            public int LoopTime { get; set; }
        }
        #endregion

        protected readonly PluginSettings settings;

        private bool _isPressed = false;

        private readonly XConnector _connector;
        public PressHoldLoopAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public override async void KeyPressed(KeyPayload payload)
        {
            _isPressed = true;
            var command = new XPlaneCommand(settings.Command, "Userdefined command");
            _connector.SendCommand(command);
            await ButtonPressing(command);

        }

        public override void KeyReleased(KeyPayload payload)
        {
            _isPressed = false;
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

        private async Task ButtonPressing(XPlaneCommand command)
        {
            await Task.Delay(settings.WaitTime); // 0.5 sec delay before the loop starts

            if (!_isPressed) // if button is released before 0.5 sec
            {
                return;
            }

            var timer = new System.Timers.Timer(settings.LoopTime);
            timer.Elapsed += (sender, e) => _connector.SendCommand(command);
            timer.Start();

            await Task.Run(() =>
            {
                while (_isPressed)
                {
                    Thread.Sleep(settings.LoopTime);
                }

                timer.Stop();
            });
        }

    }
}
