using System.Drawing;

using BarRaider.SdTools;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XDeck.Backend;

using XPlaneConnector.Core;

namespace XDeck.Actions
{
    [PluginActionId("com.valtteri.lightstatus")]
    public class LightStatusAction : KeypadBase
    {
        #region Settings

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new()
                {
                    Dataref = "sim/none/none",
                    Frequency = 5,
                    ImageMode = false,
                    TitleMode = true,
                    TitleImageMode = false,
                    Image0 = null,
                    Image1 = null,
                    Image2 = null,
                    Title0 = "0",
                    Title1 = "1",
                    Title2 = "2",
                };

                return instance;
            }

            [JsonProperty(PropertyName = "dataref")]
            public string Dataref { get; set; } = "sim/none/none";

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; } = 5;

            [JsonProperty(PropertyName = "modeImage")]
            public bool ImageMode { get; set; } = false;

            [JsonProperty(PropertyName = "modeTitle")]
            public bool TitleMode { get; set; } = true;

            [JsonProperty(PropertyName = "modeTitleImage")]
            public bool TitleImageMode { get; set; } = false;

            [FilenameProperty]
            [JsonProperty(PropertyName = "image0")]
            public string? Image0 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "image1")]
            public string? Image1 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "image2")]
            public string? Image2 { get; set; }

            [JsonProperty(PropertyName = "title0")]
            public string? Title0 { get; set; }

            [JsonProperty(PropertyName = "title1")]
            public string? Title1 { get; set; }

            [JsonProperty(PropertyName = "title2")]
            public string? Title2 { get; set; }
        }
        #endregion


        protected readonly PluginSettings? _settings;
        private readonly object _imageLock = new();
        private readonly XConnector _connector;
        private string? _currentDataref = null;
        private int _currentState = 0;

        protected readonly string _defaultIcon = ".\\Images\\pluginAction.png";

        private Image? _image0;
        private Image? _image1;
        private Image? _image2;

        public LightStatusAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _settings = payload.Settings == null || payload.Settings.Count == 0
                ? PluginSettings.CreateDefaultSettings()
                : payload.Settings.ToObject<PluginSettings>();

            _connector = XConnector.Instance;
            IntializeSettings();
            SubscribeDataref();
            SaveSettings();
        }


        public override void Dispose()
        {
            if (_currentDataref != null)
            {
                _connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Destructor called");

            GC.SuppressFinalize(this);
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

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            try { Tools.AutoPopulateSettings(_settings, payload.Settings); }
            catch { }
            IntializeSettings();
            SubscribeDataref();
            SaveSettings();
        }

        private void SubscribeDataref()
        {
            if (_settings == null) return;
            if (_currentDataref == _settings.Dataref) return;
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

        private void SaveSettings()
        {
            if (_settings == null) return;
            Connection.SetSettingsAsync(JObject.FromObject(_settings));
        }

    }
}