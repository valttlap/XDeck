using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.modifydataref")]
public class ModifyDatarefAction(SDConnection connection, InitialPayload payload) : DatarefActionBase<ModifyDatarefSettings>(connection, payload)
{
    private int _currentValue = 0;

    public override void KeyPressed(KeyPayload payload)
    {
        if (_settings == null) return;
        if (_settings.IncreaseDatarefMode)
        {
            HandleIncreaseMode();
        }
        else if (_settings.DecreaseDatarefMode)
        {
            HandleDecreaseMode();
        }
        else if (_settings.SetDatarefMode)
        {
            HandleSetMode();
        }
    }

    private void HandleIncreaseMode()
    {
        if (_settings == null) return;
        if (_currentDataref is null) return;
        if (int.TryParse(_settings.MaxRefValue, out int maxVal) && _currentValue < maxVal)
        {
            _connector.SetDataRefValue(_currentDataref, _currentValue + 1);
        }
    }

    private void HandleDecreaseMode()
    {
        if (_settings == null) return;
        if (_currentDataref is null) return;
        if (int.TryParse(_settings.MinRefValue, out int minVal) && _currentValue > minVal)
        {
            _connector.SetDataRefValue(_currentDataref, _currentValue - 1);
        }
    }

    private void HandleSetMode()
    {
        if (_settings == null) return;
        if (_currentDataref is null) return;
        if (int.TryParse(_settings.SetRefValue, out int setVal))
        {
            _connector.SetDataRefValue(_currentDataref, setVal);
        }
    }

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
            Description = "User defined dataref",
            Frequency = _settings.Frequency
        };
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {_settings.Dataref}");
        _connector.Subscribe(dataref, (element, val) => _currentValue = (int)val);
    }
}