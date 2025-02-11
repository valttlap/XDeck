using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.listendataref")]
public class ListenDatarefAction(SDConnection connection, InitialPayload payload) : DatarefActionBase<ListenDatarefSettings>(connection, payload)
{
    protected override void SubscribeDataref()
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