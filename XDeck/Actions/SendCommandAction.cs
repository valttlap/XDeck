using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.sendcommand")]
public class SendCommandAction(SDConnection connection, InitialPayload payload) : CommandActionBase<SendCommandSettings>(connection, payload)
{
    public override void KeyPressed(KeyPayload payload)
    {
        if (_settings == null) return;
        var command = new XPlaneCommand(_settings.Command, "Userdefined command");
        _connector.SendCommand(command);
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Sent command: {_settings.Command}");
    }
}