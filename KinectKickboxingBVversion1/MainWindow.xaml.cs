using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Windows.Resources;
using GestureDetector;
using KinectMouseController;

using System.IO;
namespace KinectKickboxingBVversion1
{
    /// <summary>
    /// Interaction logic for MenuFrm.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            
            InitializeComponent();
        }

        #region Speech controlling

        RecognizerInfo kinectRecognizerInfo;
        SpeechRecognitionEngine recognizer;

        KinectSensor myKinect = null;
        KinectAudioSource kinectSource = null;
        Stream audioStream;

        private RecognizerInfo findKinectRecognizerInfo()
        {
            var recognizers = SpeechRecognitionEngine.InstalledRecognizers();

            foreach (RecognizerInfo recInfo in recognizers)
            {
                // look at each recognizer info value to find the one that works for Kinect
                if (recInfo.AdditionalInfo.ContainsKey("Kinect"))
                {
                    string details = recInfo.AdditionalInfo["Kinect"];
                    if (details == "True" && recInfo.Culture.Name == "en-IE")
                    {
                        // If we get here we have found the info we want to use
                        return recInfo;
                    }
                }
            }
            return null;
        }

        private void createSpeechEngine()
        {
            kinectRecognizerInfo = findKinectRecognizerInfo();

            if (kinectRecognizerInfo == null)
            {
                MessageBox.Show("Kinect recognizer not found", "Kinect Speech");
                Application.Current.Shutdown();
            }

            try
            {
                recognizer = new SpeechRecognitionEngine(kinectRecognizerInfo);
            }
            catch
            {
                MessageBox.Show("Speech recognition engine could not be loaded", "Kinect Speech");
                Application.Current.Shutdown();
            }
        }

        private void buildCommands()
        {
            Choices commands = new Choices();

            commands.Add("Training");    // navigate to training game screen command
            commands.Add("Conditioning");  // navigate to conditioning game screen command
            
            GrammarBuilder grammarBuilder = new GrammarBuilder();

            grammarBuilder.Culture = kinectRecognizerInfo.Culture;
            grammarBuilder.Append(commands);

            Grammar grammar = new Grammar(grammarBuilder);

            recognizer.LoadGrammar(grammar);
        }

        private void setupAudio()
        {
            try
            {
                kinectSource = myKinect.AudioSource;
                kinectSource.BeamAngleMode = BeamAngleMode.Adaptive;
                audioStream = kinectSource.Start();
                recognizer.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(
                                                      EncodingFormat.Pcm, 16000, 16, 1,
                                                      32000, 2, null));
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                MessageBox.Show("Audio stream could not be connected", "Kinect Speech");
                Application.Current.Shutdown();
            }
        }


        private void SetupSpeechRecognition()
        {
            createSpeechEngine();
            buildCommands();
            setupAudio();

            recognizer.SpeechRecognized += recognizer_SpeechRecognized;
           
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.9f)
            {
                handleCommand(e.Result.Text);
            }
        }

        private void shutdownSpeechRecognition()
        {
            if (kinectSource != null)
                kinectSource.Stop();

            if (recognizer != null)
            {
                recognizer.RecognizeAsyncCancel();
                recognizer.RecognizeAsyncStop();
            }
        }

        






        #endregion

        #region Navigation commands

        void navigateToTraining()
        {
            var newForm = new TrainingFrm(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form. 
            
        }

        void navigateToConditioning()
        {
            var newForm = new TrainingFrm(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

        void handleCommand(string command)
        {
            //commandTextBlock.Text = "Command: " + command;

            switch (command)
            {
                
                case "Training":
                    navigateToTraining();
                    break;
                case "Conditioning":
                    navigateToTraining();
                    break;
            }
        }







        #endregion

        #region Button click events
        private void trainingBtn_Click(object sender, RoutedEventArgs e)
        {
            var newForm = new TrainingFrm(); //create your new form.
            myKinect.Stop();
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

        private void conditioningBtn_Click(object sender, RoutedEventArgs e)
        {
            var newForm = new ConditioningFrm(); //create your new form.
            myKinect.Stop();
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

#endregion 

        #region event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SetupSpeechRecognition();
            myKinect = KinectSensor.KinectSensors[0];
            myKinect.ColorStream.Enable();
            myKinect.ColorFrameReady += myKinect_ColorFrameReady;
            myKinect.Start();
            

        }
        #endregion

        #region Video stream

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {

                if (colorFrame == null) return;
                byte[] colorData = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(colorData);

                KinectVideo.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96,
                    PixelFormats.Bgr32, null, colorData, colorFrame.Width * colorFrame.BytesPerPixel); 

            }
        }
        #endregion
    }
}
