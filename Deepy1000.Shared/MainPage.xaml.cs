using Deepy1000.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Newtonsoft.Json;
using DeepPavlov.Dream.Schemas;
using System.Net.Mime;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Deepy1000
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isRecording;
        private LowLagMediaRecording _mediaRecording;
        StorageFile inputWavFile;
        private HttpClient httpClient;
        private string userId;
        // private Stream stream = new MemoryStream();
        private CancellationTokenSource cts;
        MediaEncodingProfile mediaProfile;
        MediaPlayer mediaPlayer;

        // images for Gerty
        BitmapImage gerty_big_smile_image;
        BitmapImage gerty_confused_image;
        BitmapImage gerty_nervous_1_image;
        BitmapImage gerty_nervous_2_image;
        BitmapImage gerty_neutral_image;
        BitmapImage gerty_sad_image;
        BitmapImage gerty_smile_image;
        BitmapImage gerty_tongue_image;
        BitmapImage gerty_very_sad_image;
        BitmapImage gerty_wink_image;
        BitmapImage gerty_worried_image;

        public MainPage()
        {
            this.InitializeComponent();

            this.HumanInputTextBox.KeyDown += HumanInputTextBox_KeyDown;

            userId = Guid.NewGuid().ToString("N");

            mediaProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            // 16000 KHz
            mediaProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

            // Media Player
            mediaPlayer = new MediaPlayer();

            // Assets for Images
            gerty_big_smile_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_big_smile.png"));
            gerty_confused_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_confused.png"));
            gerty_nervous_1_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_nervous_1.png"));
            gerty_nervous_2_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_nervous_2.png"));
            gerty_neutral_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_neutral.png"));
            gerty_sad_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_sad.png"));
            gerty_smile_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_smile.png"));
            gerty_tongue_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_tongue.png"));
            gerty_very_sad_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_very_sad.png"));
            gerty_wink_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_wink.png"));
            gerty_worried_image =
                     new BitmapImage(new Uri("ms-appx:///Assets/gerty_worried.png"));

        }

        private async void HumanInputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Tuple<StorageFile, string, string, string> result = await SendCallToAgentAsync();

                ShowResponse(result);
            }
        }

        private async Task<Tuple<StorageFile, string, string, string>> SendCallToAgentAsync()
        {
            Tuple<StorageFile, string, string, string> result = null;

            //Create an HTTP client object
            if (httpClient == null)
                httpClient = new Windows.Web.Http.HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://10.11.1.41:4242/");

            var body = new Dictionary<string, string>();
            body["user_id"] = userId;
            body["payload"] = HumanInputTextBox.Text;

            var json = JsonConvert.SerializeObject(body);

            // preparing for HTTP transfer
            HttpStringContent stringContent = new HttpStringContent(json, UnicodeEncoding.Utf8, "application/json");

            // preparing POST request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // 100 sec
            cts = new CancellationTokenSource(100000);

            HttpResponseMessage response = await httpClient.PostAsync(requestUri, stringContent).AsTask(cts.Token);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var responseContent = JsonConvert.DeserializeObject<Dialog>(responseString);

            string emotion = GetTopEmotionFromClassification(responseContent.DebugOutput[0].Annotations.EmotionClassification);

            result = new Tuple<StorageFile, string, string, string>(null, HumanInputTextBox.Text, responseContent.Response, emotion);

            return result;
        }

        private string GetTopEmotionFromClassification(EmotionClassification emotionClassification)
        {
            if (emotionClassification == null)
                return "neutral";

            Dictionary<string, double> emotions = new Dictionary<string, double>();
            emotions.Add("anger", emotionClassification.Anger);
            emotions.Add("fear", emotionClassification.Fear);
            emotions.Add("joy", emotionClassification.Joy);
            emotions.Add("Love", emotionClassification.Love);
            emotions.Add("sadness", emotionClassification.Sadness);
            emotions.Add("surprise", emotionClassification.Surprise);
            emotions.Add("neutral", emotionClassification.Neutral);

            return emotions.OrderByDescending(i => i.Value).FirstOrDefault().Key;
        }

        private async void DictateButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaCapture==null)
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                mediaCapture.Failed += MediaCapture_Failed;
                mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
            }

            if (isRecording)
            {
                await _mediaRecording.StopAsync();

                isRecording = false;

                // change logo to ...recording
                BeginRecordingPath.Visibility = Visibility.Visible;
                RecordingInProgressPath.Visibility = Visibility.Collapsed;

                // obtained file
                if (inputWavFile == null)
                {
                    // log
                    return;
                }

                var result = await SendCallToAgentProxyAsync(inputWavFile);
                if (result!=null)
                {
                    mediaPlayer.Source = MediaSource.CreateFromStorageFile(result.Item1);
                    mediaPlayer.Play();

                    ShowResponse(result);
                }
            }
            else
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                inputWavFile = await localFolder.CreateFileAsync("audio.wav", CreationCollisionOption.GenerateUniqueName);
                _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                        mediaProfile, inputWavFile);
                await _mediaRecording.StartAsync();

                isRecording = true;

                // change logo to ...recording
                BeginRecordingPath.Visibility = Visibility.Collapsed;
                RecordingInProgressPath.Visibility = Visibility.Visible;
            }
        }

        private void ShowResponse(Tuple<StorageFile, string, string, string> result)
        {
            // we shall also show what was recognized
            this.HumanInputTextBox.Text = result.Item2.ToUpper();

            // we shall also show bot's response
            this.BotResponseTextBlock.Text = result.Item3.ToUpper();

            // we shall also reflect recognized information shown as image
            var botFeelings = GetDeepyEmotionalState(result.Item3, result.Item4);

            switch (botFeelings)
            {
                case BehaviorConstants.DeepyEmotions.gerty_tongue:
                    DeepyEmotionalStateImage.Source = this.gerty_tongue_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_wink:
                    DeepyEmotionalStateImage.Source = this.gerty_wink_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_big_smile:
                    DeepyEmotionalStateImage.Source = this.gerty_big_smile_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_smile:
                    DeepyEmotionalStateImage.Source = this.gerty_smile_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_neutral:
                    DeepyEmotionalStateImage.Source = this.gerty_neutral_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_confused:
                    DeepyEmotionalStateImage.Source = this.gerty_confused_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_worried:
                    DeepyEmotionalStateImage.Source = this.gerty_worried_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_sad:
                    DeepyEmotionalStateImage.Source = this.gerty_sad_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_nervous_1:
                    DeepyEmotionalStateImage.Source = this.gerty_nervous_1_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_nervous_2:
                    DeepyEmotionalStateImage.Source = this.gerty_nervous_2_image;
                    break;
                case BehaviorConstants.DeepyEmotions.gerty_very_sad:
                    DeepyEmotionalStateImage.Source = this.gerty_very_sad_image;
                    break;
                default:
                    DeepyEmotionalStateImage.Source = this.gerty_neutral_image;
                    break;
            }
        }

        private BehaviorConstants.DeepyEmotions GetDeepyEmotionalState(string botUtteranceOriginal, string emotion)
        {
            var botUtterance = botUtteranceOriginal.ToLowerInvariant();

            if (botUtterance.Contains("i don't know what to answer"))
                return BehaviorConstants.DeepyEmotions.gerty_nervous_1;
            else if (botUtterance.Contains("i don't have this information"))
                return BehaviorConstants.DeepyEmotions.gerty_nervous_2;
            else if (botUtterance.Contains("i don't understand you"))
                return BehaviorConstants.DeepyEmotions.gerty_confused;
            else
            {
                // small override
                if (emotion == "neutral") return BehaviorConstants.DeepyEmotions.gerty_smile;
                if (emotion == "anger") return BehaviorConstants.DeepyEmotions.gerty_worried;
                if (emotion == "fear") return BehaviorConstants.DeepyEmotions.gerty_nervous_1;
                if (emotion == "joy") return BehaviorConstants.DeepyEmotions.gerty_big_smile;
                if (emotion == "love") return BehaviorConstants.DeepyEmotions.gerty_wink;
                if (emotion == "sadness") return BehaviorConstants.DeepyEmotions.gerty_very_sad;
                if (emotion == "surprise") return BehaviorConstants.DeepyEmotions.gerty_tongue;
            }

            return BehaviorConstants.DeepyEmotions.gerty_smile;
        }

        private async Task<Tuple<StorageFile, string, string, string>> SendCallToAgentProxyAsync(StorageFile inputWavFile)
        {
            Tuple<StorageFile, string, string, string> result = null;

            StorageFile outputFile = null;

            //Create an HTTP client object
            if (httpClient == null)
                httpClient = new Windows.Web.Http.HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://10.11.1.41:4343/asr?user_id=" + userId);

            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            if (inputWavFile != null)
            {
                // Ensure the stream is disposed once the image is loaded
                using (IRandomAccessStream fileStream = await inputWavFile.OpenAsync(FileAccessMode.Read))
                {
                    try
                    {

                        // preparing for HTTP transfer
                        HttpStreamContent streamContent = new HttpStreamContent(fileStream);

                        // preparing POST request
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                        HttpMultipartFormDataContent form = new HttpMultipartFormDataContent();
                        form.Add(streamContent, "file", inputWavFile.Path);

                        // 100 sec
                        cts = new CancellationTokenSource(100000);

                        HttpResponseMessage response = await httpClient.PostAsync(requestUri, form).AsTask(cts.Token);
                        response.EnsureSuccessStatusCode();

                        var responseHeaders = response.Headers;
                        var humanUtteranceTranscript = responseHeaders["transcript"];
                        var botUtteranceResponse = responseHeaders["response"];
                        var emotion = responseHeaders["emotion"];

                        // preparing output file
                        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                        outputFile = await localFolder.CreateFileAsync("output.wav", CreationCollisionOption.GenerateUniqueName);

                        using (var outputFileStream = await outputFile.OpenStreamForWriteAsync())
                        {
                            using (Stream responseStream = (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                            {
                                responseStream.CopyTo(outputFileStream);
                            }
                        }

                        result = new Tuple<StorageFile, string, string, string>(outputFile, humanUtteranceTranscript, botUtteranceResponse, emotion);
                    }
                    catch (Exception ex)
                    {
                        httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;

                        MessageDialog messageDialog = new MessageDialog("http response error: " + httpResponseBody);
                        await messageDialog.ShowAsync();

                        return null;
                    }
                }
            }
            else
            {
                MessageDialog messageDialog = new MessageDialog("Input file was null");
                await messageDialog.ShowAsync();

                return null;
            }

            return result;
        }

        private void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
        {
            //throw new NotImplementedException();

            // To Do: Show Error Message
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            // To Do: Show Error Message
        }
    } // class
} // namespace
