using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.presshold")]
public class PressHoldAction(SDConnection connection, InitialPayload payload) : CommandActionBase<PressHoldSettings>(connection, payload)
{
    private CancellationTokenSource? _cancelToken;

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
}