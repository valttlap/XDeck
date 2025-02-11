using BarRaider.SdTools;

using XDeck.Backend;
using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.listendataref")]
    public class ListenDatarefAction : KeypadBase
    {
        protected readonly ListenDatarefSettings? _settings;

        private readonly XConnector _connector;
        private string? _currentDataref;

        public ListenDatarefAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _settings = payload.Settings == null || payload.Settings.Count == 0
                ? new()
                : payload.Settings.ToObject<ListenDatarefSettings>();
            _connector = XConnector.Instance;
            SubscribeDataref();
        }


        public override void Dispose()
        {
            if (_currentDataref != null)
            {
                _connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");

            GC.SuppressFinalize(this);
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
            try
            {
                Tools.AutoPopulateSettings(_settings, payload.Settings);
            }
            catch { }
            SubscribeDataref();
        }

        private void SubscribeDataref()
        {
            if (_settings == null) return;
            if (_currentDataref != null)
            {
                _connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            _currentDataref = _settings.Dataref;

            var dataref = new DataRefElement
            {
                DataRef = _settings.Dataref,
                Units = "Unknown",
                Description = "Userdefined datared",
                Frequency = _settings.Frequency
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {_settings.Dataref}");
            _connector.Subscribe(dataref, async (element, val) =>
            {
                await Connection.SetTitleAsync($"{val}  {_settings.Units}");
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribed dataref: {_settings.Dataref}");
            });
        }
    }
}