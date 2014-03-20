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
        #region local variables
        // Matcher
        private GestureMatcher matcher;

        RecognizerInfo kinectRecognizerInfo;
        KinectSensor myKinect = null;

        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        #endregion

        // Path to gesture. Point this to any GesturePak created gesture
        private string gesturefile =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GesturePak\\wave.xml";

         public TrainingFrm()
        {
            
            InitializeComponent();
        }

        #region drawing skeleton

         /// <summary>
         /// Draws indicators to show which edges are clipping skeleton data
         /// </summary>
         /// <param name="skeleton">skeleton to draw clipping information for</param>
         /// <param name="drawingContext">drawing context to draw to</param>
         private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
         {
             if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
             {
                 drawingContext.DrawRectangle(
                     Brushes.Red,
                     null,
                     new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
             }

             if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
             {
                 drawingContext.DrawRectangle(
                     Brushes.Red,
                     null,
                     new Rect(0, 0, RenderWidth, ClipBoundsThickness));
             }

             if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
             {
                 drawingContext.DrawRectangle(
                     Brushes.Red,
                     null,
                     new Rect(0, 0, ClipBoundsThickness, RenderHeight));
             }

             if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
             {
                 drawingContext.DrawRectangle(
                     Brushes.Red,
                     null,
                     new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
             }
         }

         /// <summary>
         /// Event handler for Kinect sensor's SkeletonFrameReady event
         /// </summary>
         /// <param name="sender">object sending the event</param>
         /// <param name="e">event arguments</param>
         void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
         {
             Skeleton[] skeletons = new Skeleton[0];

             using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
             {
                 if (skeletonFrame != null)
                 {
                     skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                     skeletonFrame.CopySkeletonDataTo(skeletons);
                 }
             }

             using (DrawingContext dc = this.drawingGroup.Open())
             {
                 // Draw a transparent background to set the render size
                 dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                 if (skeletons.Length != 0)
                 {
                     foreach (Skeleton skel in skeletons)
                     {
                         RenderClippedEdges(skel, dc);

                         if (skel.TrackingState == SkeletonTrackingState.Tracked)
                         {
                             this.DrawBonesAndJoints(skel, dc);
                         }
                         else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                         {
                             dc.DrawEllipse(
                             this.centerPointBrush,
                             null,
                             this.SkeletonPointToScreen(skel.Position),
                             BodyCenterThickness,
                             BodyCenterThickness);
                         }
                     }
                 }

                 // prevent drawing outside of our render area
                 this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
             }

         }
          
         

         /// <summary>
         /// Draws a skeleton's bones and joints
         /// </summary>
         /// <param name="skeleton">skeleton to draw</param>
         /// <param name="drawingContext">drawing context to draw to</param>
         private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
         {
             // Render Torso
             this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
             this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
             this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
             this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
             this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
             this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
             this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

             // Left Arm
             this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
             this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
             this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

             // Right Arm
             this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
             this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
             this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

             // Left Leg
             this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
             this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
             this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

             // Right Leg
             this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
             this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
             this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

             // Render Joints
             foreach (Joint joint in skeleton.Joints)
             {
                 Brush drawBrush = null;

                 if (joint.TrackingState == JointTrackingState.Tracked)
                 {
                     drawBrush = this.trackedJointBrush;
                 }
                 else if (joint.TrackingState == JointTrackingState.Inferred)
                 {
                     drawBrush = this.inferredJointBrush;
                 }

                 if (drawBrush != null)
                 {
                     drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                 }
             }
         }

         /// <summary>
         /// Maps a SkeletonPoint to lie within our render space and converts to Point
         /// </summary>
         /// <param name="skelpoint">point to map</param>
         /// <returns>mapped point</returns>
         private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
         {
             // Convert point to depth space.  
             // We are not using depth directly, but we do want the points in our 640x480 output resolution.
             DepthImagePoint depthPoint = this.myKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
             return new Point(depthPoint.X, depthPoint.Y);
         }

         /// <summary>
         /// Draws a bone line between two joints
         /// </summary>
         /// <param name="skeleton">skeleton to draw bones from</param>
         /// <param name="drawingContext">drawing context to draw to</param>
         /// <param name="jointType0">joint to start drawing from</param>
         /// <param name="jointType1">joint to end drawing at</param>
         private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
         {
             Joint joint0 = skeleton.Joints[jointType0];
             Joint joint1 = skeleton.Joints[jointType1];

             // If we can't find either of these joints, exit
             if (joint0.TrackingState == JointTrackingState.NotTracked ||
                 joint1.TrackingState == JointTrackingState.NotTracked)
             {
                 return;
             }

             // Don't draw if both points are inferred
             if (joint0.TrackingState == JointTrackingState.Inferred &&
                 joint1.TrackingState == JointTrackingState.Inferred)
             {
                 return;
             }

             // We assume all drawn bones are inferred unless BOTH joints are tracked
             Pen drawPen = this.inferredBonePen;
             if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
             {
                 drawPen = this.trackedBonePen;
             }

             drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
         }


        #endregion


         #region methods

         private void PlaySound()
        {
            Uri uri = new Uri(@"pack://application:,,,/Sounds/jabSound.wav");
            var player = new MediaPlayer();
            player.Open(uri);
            player.Play();
        }

        #endregion




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

            //events
            matcher.StartedRecognizing += matcher_StartedRecognizing;
            matcher.DoneRecognizing += matcher_DoneRecognizing;
            matcher.Tracking += matcher_Tracking;
            matcher.NotTracking += matcher_NotTracking;
            matcher.GestureMatch += matcher_GestureMatch;

            // Start recognizing your gestures!
            matcher.StartRecognizing();

            //Method that initializes the skeleton tracking
            SkeletonInitilization();

            
            
        }

        private void SkeletonInitilization()
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            KinectSkeletonFeed.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.myKinect = potentialSensor;
                    break;
                }
            }

            if (null != this.myKinect)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.myKinect.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.myKinect.SkeletonFrameReady += myKinect_SkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.myKinect.Start();
                }
                catch (IOException)
                {
                    this.myKinect = null;
                }
            }

            if (null == this.myKinect)
            {
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        

        void matcher_GestureMatch(Gesture gesture)
        {
            lblGestureMatch.Content = gesture.Name;
            PlaySound();

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
