using BarRaider.SdTools;

using Newtonsoft.Json.Linq;

using XDeck.Backend;

namespace XDeck.Actions;
public abstract class CommandActionBase<TSettings> : KeypadBase
    where TSettings : class, new()
{
    protected TSettings? _settings;
    protected readonly XConnector _connector = XConnector.Instance;
    protected string? _currentDataref;

    protected CommandActionBase(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        // 1. Grab or initialize the settings
        _settings = payload.Settings == null || payload.Settings.Count == 0
            ? new TSettings()
            : payload.Settings.ToObject<TSettings>();

        _connector = XConnector.Instance;

        // 2. Initialize
        OnInit();
    }

    public override void Dispose()
    {
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");
        GC.SuppressFinalize(this);
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        // Repopulate settings
        try
        {
            Tools.AutoPopulateSettings(_settings, payload.Settings);
        }
        catch
        {
            // Gracefully ignore
        }

        // Let derived classes handle “refresh” logic
        OnSettingsUpdated();

        // Save to the property inspector
        SaveSettings();
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
        // Usually empty, but you can override in derived classes if needed
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

    protected virtual void OnInit()
    {
        // If needed, do any common initialization
        // e.g., SaveSettings() or local initialization
        SaveSettings();
    }

    protected virtual void OnSettingsUpdated()
    {
        // Let derived classes do extra logic whenever settings change
    }

    /// <summary>
    /// This is a common helper to save settings back to the PI.
    /// </summary>
    protected virtual void SaveSettings()
    {
        if (_settings == null) return;
        Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }
}
