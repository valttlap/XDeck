using System.Drawing;

using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.multilistener")]
public class MultiListenerAction(ISDConnection connection, InitialPayload payload) : DatarefActionBase<MultiListenerSettings>(connection, payload)
{
    private readonly object _imageLock = new();
    private int? _currentValue = 0;

    protected override void OnInit()
    {
        InitializeSettings();
        base.OnInit();
    }

    protected override void OnSettingsUpdated()
    {
        InitializeSettings();
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
            _currentValue = (int)val;
            await SetImageTitleAsync();
        });
    }

    private async Task SetImageTitleAsync()
    {
        if (_settings == null || _settings.Settings.Count == 0)
        {
            await SetDefaultsAsync();
            return;
        }

        if (!_settings.Settings.TryGetValue(_currentValue ?? 0, out var imageTitle))
        {
            await SetDefaultsAsync();
            return;
        }

        var tasks = new List<Task>();

        if (imageTitle.Image != null)
        {
            tasks.Add(Connection.SetImageAsync(imageTitle.Image));
        }
        else
        {
            tasks.Add(Connection.SetDefaultImageAsync());
        }

        if (!string.IsNullOrEmpty(imageTitle.Title))
        {
            tasks.Add(Connection.SetTitleAsync(imageTitle.Title));
        }
        else
        {
            tasks.Add(Connection.SetTitleAsync(""));
        }

        if (tasks.Count == 0)
        {
            await SetDefaultsAsync();
            return;
        }

        await Task.WhenAll(tasks);
    }

    // Helper method to set default image and title concurrently.
    private async Task SetDefaultsAsync()
    {
        await Task.WhenAll(Connection.SetDefaultImageAsync(), Connection.SetTitleAsync("Unknown"));
    }

    protected virtual void InitializeSettings()
    {
        PrefetchImages();
    }

    private void PrefetchImages()
    {
        if (_settings == null) return;
        if (_settings.Settings == null || _settings.Settings.Count == 0) return;
        lock (_imageLock)
        {
            foreach (var dataref in _settings.Settings)
            {
                dataref.Value.Image = LoadImage(dataref.Value.ImagePath);
            }
        }
    }

    private static Image? LoadImage(string? imagePath)
    {
        return string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath) ? null : Image.FromFile(imagePath);
    }
}