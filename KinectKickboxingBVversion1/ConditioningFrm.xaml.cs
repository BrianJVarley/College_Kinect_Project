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
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Windows.Resources;



using System.IO;

namespace KinectKickboxingBVversion1
{

    /// <summary>
    /// Interaction logic for ConditioningFrm.xaml
    /// </summary>
    public partial class ConditioningFrm : Window
    {
        KinectSensor myKinect = null;
        public ConditioningFrm()
        {    
            InitializeComponent();
        }

        

        

        #region button click events
        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

        private void pushUpBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void burpeeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void squatBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void coreBtn_Click(object sender, RoutedEventArgs e)
        {

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
        #region event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            myKinect = KinectSensor.KinectSensors[0];
            myKinect.ColorStream.Enable();
            myKinect.ColorFrameReady +=myKinect_ColorFrameReady;
            myKinect.Start();

        }

        #endregion
    }
}
