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
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Resources;
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;
using GesturePak;

namespace KinectKickboxingBVversion1
{
    /// <summary>
    /// Interaction logic for TrainingFrm.xaml
    /// </summary>
    public partial class TrainingFrm : Window
    {
        // Matcher
        private GestureMatcher matcher;

        // Path to gesture. Point this to any GesturePak created gesture
        private string gesturefile =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GesturePak\\jab.xml";

         public TrainingFrm()
        {
            
            InitializeComponent();
        }

         RecognizerInfo kinectRecognizerInfo;
         SpeechRecognitionEngine recognizer;

         KinectSensor myKinect = null;

        

        

        #region button click events
        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

        private void jabBtn_Click(object sender, RoutedEventArgs e)
        {
            lblScore.Visibility = Visibility.Visible;
        }

        private void crossBtn_Click(object sender, RoutedEventArgs e)
        {
            lblScore.Visibility = Visibility.Visible;
        }

        private void jabCrossHookBtn_Click(object sender, RoutedEventArgs e)
        {
            lblScore.Visibility = Visibility.Visible;
        }

        private void pKickBtn_Click(object sender, RoutedEventArgs e)
        {
            lblScore.Visibility = Visibility.Visible;
        }

        private void blockBtn_Click(object sender, RoutedEventArgs e)
        {
            lblScore.Visibility = Visibility.Visible;
        }

        #endregion

        
        #region event handlers

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure we have a Kinect
		    if (KinectSensor.KinectSensors.Count == 0) {
			    MessageBox.Show("Please plug in your Kinect and try again");
			    Application.Current.Shutdown();
		    }

            // Make sure we have a gesture file
            if (System.IO.File.Exists(gesturefile) == false)
            {
                MessageBox.Show("Please modify this code to point to an existing gesture file.");
                Application.Current.Shutdown();
            }

            // Create your gesture objects (however many you want to test)
            Gesture g1 = new Gesture(gesturefile);

            // Add it to a gestures collection
            List<Gesture> gestures = new List<Gesture>();
            gestures.Add(g1);
            myKinect = KinectSensor.KinectSensors[0];
            myKinect.Start();

            // Create a new matcher from the Kinect sensor and the gestures
            matcher = new GestureMatcher(KinectSensor.KinectSensors[0], gestures);

            // hook up events
            matcher.StartedRecognizing += matcher_StartedRecognizing;
            matcher.DoneRecognizing += matcher_DoneRecognizing;
            matcher.Tracking += matcher_Tracking;
            matcher.NotTracking += matcher_NotTracking;
            matcher.PoseMatch += matcher_PoseMatch;
            matcher.GestureMatch += matcher_GestureMatch;

            // Start recognizing your gestures!
            matcher.StartRecognizing();


            

        }

        void matcher_GestureMatch(Gesture gesture)
        {
            // We got a match!
            lblGestureMatch.Content = gesture.Name;
        }

        void matcher_PoseMatch(MatchingPose match, Pose pose)
        {
            //  We have matched a pose. 
            //  match.Pose is the pose from the gesture
            //  pose is the current frame (real time)
        }

        void matcher_NotTracking()
        {
            // The window goes white when not tracking
            this.Background = Brushes.White;
        }

        void matcher_Tracking(Pose pose, float delta)
        {
            // The window goes red when tracking
            this.Background = Brushes.Red;
        }

        void matcher_DoneRecognizing()
        {
            // This tells us the matcher is NOT recognizing
            lblGestureMatch.Content = "Not Watching";
        }

        void matcher_StartedRecognizing()
        {
            // This tells us the matcher is recognizing
            lblGestureMatch.Content = "Watching...";
        }
        #endregion

    }
}
