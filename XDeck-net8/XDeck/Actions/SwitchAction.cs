using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using XDeck.Backend;
using XPlaneConnector.Core;

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
            public string Dataref { get; set; } = "sim/none/none";

            [JsonProperty(PropertyName = "pollFreq")]
            public int Frequency { get; set; } = 5;

            [JsonProperty(PropertyName = "command")]
            public string? Command { get; set; }

            [JsonProperty(PropertyName = "modeCommand")]
            public bool CommandMode { get; set; } = false;

            [JsonProperty(PropertyName = "modeDataref")]
            public bool DatarefMode { get; set; } = true;

            [JsonProperty(PropertyName = "modeImage")]
            public bool ImageMode { get; set; } = false;

            [JsonProperty(PropertyName = "modeTitle")]
            public bool TitleMode { get; set; } = true;

            [JsonProperty(PropertyName = "modeTitleImage")]
            public bool TitleImageMode { get; set; } = false;

            [FilenameProperty]
            [JsonProperty(PropertyName = "offImage")]
            public string? OffImage { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "onImage")]
            public string? OnImage { get; set; }

            [JsonProperty(PropertyName = "offTitle")]
            public string OffTitle { get; set; } = "OFF";

            [JsonProperty(PropertyName = "onTitle")]
            public string OnTitle { get; set; } = "ON";
        }
        #endregion


        protected readonly PluginSettings? settings;
        protected string defaultOffImageLocation = ".\\Images\\empty_button_off.png";
        protected string defaultOnImageLocation = ".\\Images\\empty_button_on.png";
        private readonly object _imageLock = new();
        private readonly XConnector _connector;
        private string? _currentDataref;
        private int _currentState = 0;

        private Image? _offImage;
        private Image? _onImage;

        public SwitchAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0) // Called the first time you drop a new action into the Stream Deck
            {
                settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }
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
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (settings == null) return;
            if (_currentDataref == null) return;
            if (settings.CommandMode)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Current mode is command");
                var command = new XPlaneCommand(settings.Command, "Userdefined command");
                _connector.SendCommand(command);
                return;
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Current mode is dataref");
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
            if (settings == null) return;
            if (_currentDataref == settings.Dataref) return;
            if (_currentDataref != null)
            {
                _connector.Unsubscribe(_currentDataref);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Unsubscribed dataref: {_currentDataref}");
            }
            _currentDataref = settings.Dataref;

            var dataref = new DataRefElement
            {
                DataRef = settings.Dataref,
                Units = "Unknown",
                Description = "Userdefined datared",
                Frequency = settings.Frequency
            };
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType()} Subscribing dataref: {settings.Dataref}");
            _connector.Subscribe(dataref, async (element, val) =>
            {
                _currentState = (int)val;
                await SetImageTitleAsync();
            });
        }

        private async Task SetImageTitleAsync()
        {
            if (settings == null) return;
            string title = _currentState == 0 ? settings.OffTitle : settings.OnTitle;
            Image? image = _currentState == 0 ? _offImage : _onImage;

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
                _offImage = LoadImage(settings?.OffImage) ?? LoadImage(defaultOffImageLocation);
                _onImage = LoadImage(settings?.OnImage) ?? LoadImage(defaultOnImageLocation);
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

        private void SaveSettings()
        {
            if (settings == null) return;
            Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

    }
}
