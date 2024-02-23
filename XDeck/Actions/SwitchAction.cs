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
    [PluginActionId("com.valtteri.switch")]
    public class SwitchAction : KeypadBase
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
                    DatarefMode = true,
                    Command = null,
                    CommandMode = false,
                    ImageMode = false,
                    TitleMode = true,
                    TitleImageMode = false,
                    OffImage = null,
                    OnImage = null,
                    OffTitle = "OFF",
                    OnTitle = "ON"
                };

                return instance;
            }

            [JsonProperty(PropertyName = "dataref")]
            public string Dataref { get; set; }

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; }

            [JsonProperty(PropertyName = "command")]
            public string Command { get; set; }

            [JsonProperty(PropertyName = "modeCommand")]
            public bool CommandMode { get; set; }

            [JsonProperty(PropertyName = "modeDataref")]
            public bool DatarefMode { get; set; }

            [JsonProperty(PropertyName = "modeImage")]
            public bool ImageMode { get; set; }

            [JsonProperty(PropertyName = "modeTitle")]
            public bool TitleMode { get; set; }

            [JsonProperty(PropertyName = "modeTitleImage")]
            public bool TitleImageMode { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "offImage")]
            public string OffImage { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "onImage")]
            public string OnImage { get; set; }

            [JsonProperty(PropertyName = "offTitle")]
            public string OffTitle { get; set; }

            [JsonProperty(PropertyName = "onTitle")]
            public string OnTitle { get; set; }
        }
        #endregion


        protected readonly PluginSettings settings;
        protected string defaultOffImageLocation = ".\\Images\\empty_button_off.png";
        protected string defaultOnImageLocation = ".\\Images\\empty_button_on.png";
        private readonly object _imageLock = new object();
        private readonly XConnector _connector;
        private string _currentDataref = null;
        private int _currentState = 0;

        private Image _offImage;
        private Image _onImage;

        public SwitchAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (settings.CommandMode)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Current mode is command");
                var command = new XPlaneCommand(settings.Command, "Userdefined command");
                _connector.SendCommand(command);
                return;
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Current mode is dataref");
            float newVal = (_currentState == 0) ? 1 : 0;
            _connector.SetDataRefValue(_currentDataref, newVal);
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
            SetImageTitleAsync().Wait();
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
            string title = _currentState == 0 ? settings.OffTitle : settings.OnTitle;
            Image image = _currentState == 0 ? _offImage : _onImage;

            if (settings.TitleMode)
            {
                var setTitleTask = Connection.SetTitleAsync(title);
                var setImageTask = Connection.SetDefaultImageAsync();

                await Task.WhenAll(setTitleTask, setImageTask);
            }
            else if (settings.ImageMode)
            {
                var setImageTask = Connection.SetImageAsync(image);
                var setTitleTask = Connection.SetTitleAsync("");

                await Task.WhenAll(setImageTask, setTitleTask);

            }
            else if (settings.TitleImageMode)
            {
                var setImageTask = Connection.SetImageAsync(image);
                var setTitleTask = Connection.SetTitleAsync(title);

                await Task.WhenAll(setImageTask, setTitleTask);
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
                _offImage = LoadImage(settings.OffImage) ?? LoadImage(defaultOffImageLocation);
                _onImage = LoadImage(settings.OnImage) ?? LoadImage(defaultOnImageLocation);
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
