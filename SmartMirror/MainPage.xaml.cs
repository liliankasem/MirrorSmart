using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace SmartMirror
{
    public sealed partial class MainPage : Page
    {
        string userName;
        private CoreDispatcher dispatcher;
        private SpeechSynthesizer synthesizer;
        private FaceDetector faceDetector;

        // Speech recognizition
        private static uint HResultRecognizerNotFound = 0x8004503a;
        private static int NoCaptureDevicesHResult = -1072845856;
        private SpeechRecognizer speechRecognizer;
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Need for project oxford configuration
            //SetupPersonGroup();

            //Setup all items
            TimeUpdater();
            await GetWeather();

            this.cameraControl.SetFaceProcessor(this.ProcessVideoFrame);

            DispatcherTimerSetup();

            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            bool permissionGained = await RequestMicrophonePermission();

            if (!permissionGained)
                return; // No permission granted

            Language speechLanguage = SpeechRecognizer.SystemSpeechLanguage;
            string langTag = speechLanguage.LanguageTag;
            speechContext = ResourceContext.GetForCurrentView();
            speechContext.Languages = new string[] { langTag };

            speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationSpeechResources");

            await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);

            try
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                await messageDialog.ShowAsync();
            }
        }

        async Task ProcessVideoFrame(SoftwareBitmap bitmap)
        {
            if (this.faceDetector == null)
            {
                this.faceDetector = await FaceDetector.CreateAsync();
            }

            var results = await this.faceDetector.DetectFacesAsync(bitmap);
            var faceFound = results?.Count > 0;

            if (faceFound)
            {
                cameraControl.faceProcessingPaused = true;

                var user = await this.cameraControl.Snap();
                userName = await IdentifyUser(user);

                if (userName == "Lilian")
                    await GetEventsGmail();

               await Task.Delay(10000);
               cameraControl.faceProcessingPaused = false;
            }
            else
            {
                eventsList_txt.Text = "";
                IdentityTextBlock.Text = "";
            }
        }

        /// <summary>
        /// Below is the voice actions
        /// </summary>

        private async void CheckTime()
        {
            string time = string.Format("{0:hh:mm tt}", DateTime.Now);
            Debug.WriteLine(time);
            await Speak("The time is " + time);
        }

        private async void CheckDate()
        {
            DateTime moment = DateTime.Now;
            int year = moment.Year;
            string month = moment.ToString("MMMM");
            int day = moment.Day;

            string date = moment.DayOfWeek + ", " + day + " " + month + " " + year;

            Debug.WriteLine(date);
            await Speak("Today is " + date);
        }

        private async void CheckRoom()
        {
            await Speak("Checking room");

            string page = "http://spikeapi.azurewebsites.net/api/values/";
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(page))
                using (HttpContent content = response.Content)
                {
                    // ... Read the string.
                    string result = await content.ReadAsStringAsync();

                    // ... Display the result.
                    //resultTextBlock.Visibility = Visibility.Visible;
                    //resultTextBlock.Text = result;
                    await Speak(result);

                    if (result != null &&
                        result.Length >= 50)
                    {
                        Debug.WriteLine(result.Substring(0, 50) + "...");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}