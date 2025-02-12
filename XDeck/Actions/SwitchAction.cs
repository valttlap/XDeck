using System.Drawing;

using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.switch")]
public class SwitchAction(ISDConnection connection, InitialPayload payload) : DatarefActionBase<SwitchSettings>(connection, payload)
{
    protected string _defaultOffImageLocation = ".\\Images\\empty_button_off.png";
    protected string _defaultOnImageLocation = ".\\Images\\empty_button_on.png";
    private readonly object _imageLock = new();
    private int _currentState = 0;

    private Image? _offImage;
    private Image? _onImage;

    protected override void OnInit()
    {
        PrefetchImages();
        base.OnInit();
    }

    protected override void OnSettingsUpdated()
    {
        PrefetchImages();
    }

    public override void KeyPressed(KeyPayload payload)
    {
        if (_settings == null) return;
        if (_currentDataref == null) return;
        if (_settings.CommandMode)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Current mode is command");
            var command = new XPlaneCommand(_settings.Command, "Userdefined command");
            _connector.SendCommand(command);
            return;
        }
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Current mode is dataref");
        float newVal = (_currentState == 0) ? 1 : 0;
        _connector.SetDataRefValue(_currentDataref, newVal);
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        try { Tools.AutoPopulateSettings(_settings, payload.Settings); }
        catch { }
        PrefetchImages();
        SubscribeDataref();
        SaveSettings();
        SetImageTitleAsync().Wait();
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
            Description = "Userdefined datared",
            Frequency = _settings.Frequency
        };
        Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {_settings.Dataref}");
        _connector.Subscribe(dataref, async (element, val) =>
        {
            _currentState = (int)val;
            await SetImageTitleAsync();
        });
    }

    private async Task SetImageTitleAsync()
    {
        if (_settings == null) return;
        string title = _currentState == 0 ? _settings.OffTitle : _settings.OnTitle;
        Image? image = _currentState == 0 ? _offImage : _onImage;

        if (_settings.TitleMode)
        {
            var setTitleTask = Connection.SetTitleAsync(title);
            var setImageTask = Connection.SetDefaultImageAsync();

            await Task.WhenAll(setTitleTask, setImageTask);
        }
        else if (_settings.ImageMode)
        {
            var setImageTask = Connection.SetImageAsync(image);
            var setTitleTask = Connection.SetTitleAsync("");

            await Task.WhenAll(setImageTask, setTitleTask);

        }
        else if (_settings.TitleImageMode)
        {
            var setImageTask = Connection.SetImageAsync(image);
            var setTitleTask = Connection.SetTitleAsync(title);

            await Task.WhenAll(setImageTask, setTitleTask);
        }
    }

    private void PrefetchImages()
    {
        lock (_imageLock)
        {
            _offImage = LoadImage(_settings?.OffImage) ?? LoadImage(_defaultOffImageLocation);
            _onImage = LoadImage(_settings?.OnImage) ?? LoadImage(_defaultOnImageLocation);
        }
    }

    private Image? LoadImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Current directory: {currentDirectory}");
            Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType()} Background image file not found {imagePath}");
            return null;
        }

#pragma warning disable CA1416 // Validate platform compatibility
        return Image.FromFile(imagePath);
#pragma warning restore CA1416 // Validate platform compatibility
    }
}