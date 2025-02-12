using BarRaider.SdTools;

using XDeck.Backend;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.connstop")]
public class StopAction(ISDConnection connection, InitialPayload payload) : KeypadBase(connection, payload)
{
    private readonly XConnector _connector = XConnector.Instance;

    public override void Dispose()
    {
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
        GC.SuppressFinalize(this);
    }

    public override void KeyPressed(KeyPayload payload)
    {
        _connector.Stop();
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