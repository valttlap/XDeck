using BarRaider.SdTools;

namespace XDeck
{
    [PluginActionId("com.valtteri.connstart")]
    public class StartAction : KeypadBase
    {
        private readonly XConnector _connector;

        #region Private Members


        #endregion
        public StartAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _connector = XConnector.Instance;
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            _connector.Connect();
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
    }
}