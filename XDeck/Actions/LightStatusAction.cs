using System.Drawing;

using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.lightstatus")]
public class LightStatusAction(ISDConnection connection, InitialPayload payload) : DatarefActionBase<LightStatusSettings>(connection, payload)
{
    private readonly object _imageLock = new();
    private int _currentState = 0;

    protected readonly string _defaultIcon = ".\\Images\\pluginAction.png";

    private Image? _image0;
    private Image? _image1;
    private Image? _image2;

    protected override void OnInit()
    {
        IntializeSettings();
        base.OnInit();
    }

    protected override void OnSettingsUpdated()
    {
        IntializeSettings();
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
        string? title;
        Image? image;

        switch (_currentState)
        {
            case 0:
                image = _image0;
                title = _settings.Title0;
                break;
            case 1:
                image = _image1;
                title = _settings.Title1;
                break;
            case 2:
                image = _image2;
                title = _settings.Title2;
                break;
            default:
                image = null;
                title = "Unknown";
                break;
        }

        if (_settings.TitleMode || _settings.TitleImageMode)
        {
            await Connection.SetTitleAsync(title);
        }

        if (_settings.ImageMode || _settings.TitleImageMode)
        {
            await Connection.SetImageAsync(image);
        }
    }

    protected virtual void IntializeSettings()
    {
        PrefetchImages();
    }

    private void PrefetchImages()
    {
        if (_settings == null) return;
        lock (_imageLock)
        {
            _image0 = LoadImage(_settings.Image0) ?? LoadImage(_defaultIcon);
            _image1 = LoadImage(_settings.Image1) ?? LoadImage(_defaultIcon);
            _image2 = LoadImage(_settings.Image2) ?? LoadImage(_defaultIcon);
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

        return Image.FromFile(imagePath);
    }

}