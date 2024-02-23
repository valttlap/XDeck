using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using XPlaneConnector;

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
                PluginSettings instance = new PluginSettings
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
            public string Dataref { get; set; }

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; }

            [JsonProperty(PropertyName = "modeImage")]
            public bool ImageMode { get; set; }

            [JsonProperty(PropertyName = "modeTitle")]
            public bool TitleMode { get; set; }

            [JsonProperty(PropertyName = "modeTitleImage")]
            public bool TitleImageMode { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "image0")]
            public string Image0 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "image1")]
            public string Image1 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "image2")]
            public string Image2 { get; set; }

            [JsonProperty(PropertyName = "title0")]
            public string Title0 { get; set; }

            [JsonProperty(PropertyName = "title1")]
            public string Title1 { get; set; }

            [JsonProperty(PropertyName = "title2")]
            public string Title2 { get; set; }
        }
        #endregion


        protected readonly PluginSettings settings;
        private readonly object _imageLock = new object();
        private readonly XConnector _connector;
        private string _currentDataref = null;
        private int _currentState = 0;

        protected readonly string defaultIcon = ".\\Images\\pluginAction.png";

        private Image _image0;
        private Image _image1;
        private Image _image2;

        public LightStatusAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0) // Called the first time you drop a new action into the Stream Deck
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            _connector = XConnector.Instance;
            IntializeSettings();
            SubscribeDataref();
            SaveSettings();
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Destructor called");
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
            try { Tools.AutoPopulateSettings(settings, payload.Settings); }
            catch { }
            IntializeSettings();
            SubscribeDataref();
            SaveSettings();
        }

        private void SubscribeDataref()
        {
            if (_currentDataref == settings.Dataref) return;
            if (_currentDataref != null)
            {
                _connector.Connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            _currentDataref = settings.Dataref;

            var dataref = new DataRefElement
            {
                DataRef = settings.Dataref,
                Units = "Unknown",
                Description = "Userdefined datared",
                Frequency = settings.Frequency
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Subscribing dataref: {settings.Dataref}");
            _connector.Connector.Subscribe(dataref, dataref.Frequency, async (element, val) =>
            {
                _currentState = (int)val;
                await SetImageTitleAsync();
            });
        }

        private async Task SetImageTitleAsync()
        {
            string title;
            Image image;

            switch (_currentState)
            {
                case 0:
                    image = _image0;
                    title = settings.Title0;
                    break;
                case 1:
                    image = _image1;
                    title = settings.Title1;
                    break;
                case 2:
                    image = _image2;
                    title = settings.Title2;
                    break;
                default:
                    image = null;
                    title = "Unknown";
                    break;
            }

            if (settings.TitleMode || settings.TitleImageMode)
            {
                await Connection.SetTitleAsync(title);
            }

            if (settings.ImageMode || settings.TitleImageMode)
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
            lock (_imageLock)
            {
                _image0 = LoadImage(settings.Image0) ?? LoadImage(defaultIcon);
                _image1 = LoadImage(settings.Image1) ?? LoadImage(defaultIcon);
                _image2 = LoadImage(settings.Image2) ?? LoadImage(defaultIcon);
            }
        }

        private Image LoadImage(string imagePath)
        {
            if (String.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Current directory: {currentDirectory}");
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{this.GetType()} Background image file not found {imagePath}");
                return null;
            }

            return Image.FromFile(imagePath);
        }

        private void SaveSettings()
        {
            Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

    }
}
