using BarRaider.SdTools;

namespace XDeck.Actions;
public abstract class DatarefActionBase<TSettings> : CommandActionBase<TSettings>
    where TSettings : class, new()
{
    protected DatarefActionBase(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        if (_connector.IsXPlaneOnline)
        {
            SubscribeDataref();
        }
        else
        {
            HandleXPlaneOffline();
        }

        _connector.OnXPlaneOnline += SubscribeDataref;
        _connector.OnConnectionLost += HandleXPlaneOffline;
    }

    public override void Dispose()
    {
        // Unsubscribe from dataref if necessary
        if (_currentDataref != null)
        {
            _connector.Unsubscribe(_currentDataref);
            Logger.Instance.LogMessage(TracingLevel.INFO,
                $"{GetType()} Unsubscribed dataref: {_currentDataref}");
        }

        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
        GC.SuppressFinalize(this);
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        base.ReceivedSettings(payload);
        SubscribeDataref();
    }

    protected virtual void HandleXPlaneOffline()
    {
        // Default: set “Not Online” or similar
        _ = Connection.SetDefaultImageAsync();
        _ = Connection.SetTitleAsync("X-Plane\nNot\nOnline");
    }

    /// <summary>
    /// Derived classes must implement how they want to subscribe 
    /// because the structure of the dataref might differ, 
    /// or you might want a different callback. 
    /// </summary>
    protected abstract void SubscribeDataref();
}
