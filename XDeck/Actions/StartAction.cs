using BarRaider.SdTools;

using XDeck.Backend;

namespace XDeck
{
    [PluginActionId("com.valtteri.connstart")]
    public class StartAction(SDConnection connection, InitialPayload payload) : KeypadBase(connection, payload)
    {
        private readonly XConnector _connector = XConnector.Instance;

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
            GC.SuppressFinalize(this);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            _connector.Restart();
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
    }
}