using BarRaider.SdTools;

using XDeck.Backend;
using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.presshold")]
    public class PressHoldAction(SDConnection connection, InitialPayload payload) : KeypadBase(connection, payload)
    {
        protected readonly PressHoldSettings? _settings = payload.Settings == null || payload.Settings.Count == 0
                ? new()
                : payload.Settings.ToObject<PressHoldSettings>();

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