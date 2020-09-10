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
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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

        public MainPage()
        {
            this.InitializeComponent();

            
            

            userId = Guid.NewGuid().ToString("N");

            mediaProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            // 16000 KHz
            mediaProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

            // Media Player
            mediaPlayer = new MediaPlayer();

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

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
                    mediaPlayer.Source = MediaSource.CreateFromStorageFile(result); 
                    mediaPlayer.Play();
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

        private async Task<StorageFile> SendCallToAgentProxyAsync(StorageFile inputWavFile)
        {
            StorageFile result = null;

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

                        // preparing output file
                        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                        result = await localFolder.CreateFileAsync("output.wav", CreationCollisionOption.GenerateUniqueName);

                        using (var outputFileStream = await result.OpenStreamForWriteAsync())
                        {
                            using (Stream responseStream = (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                            {
                                responseStream.CopyTo(outputFileStream);
                            }
                        }
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
