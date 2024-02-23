using BarRaider.SdTools;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.connstop")]
    public class StopAction : KeypadBase
    {
        private readonly XConnector _connector;
        public StopAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _connector = XConnector.Instance;
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            _connector.Disconnect();
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
        }
    }
}
