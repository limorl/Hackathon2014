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
using Microsoft.Samples.Kinect.WpfViewers;

namespace KinectSamples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor sensor;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.kinectSensorChooser.KinectSensorManager = new KinectSensorManager();
                this.kinectSensorChooser.KinectSensorManager.KinectSensorChanged += KinectSensorManager_KinectSensorChanged;

                // set the Kinect sensor
                this.kinectSensorChooser.KinectSensorManager.KinectSensor = KinectSensor.KinectSensors[0];
                
                //this.sensor = KinectSensor.KinectSensors[0];
                //if (this.sensor.Status == KinectStatus.Connected)
                //{
                //    this.sensor.ColorStream.Enable();
                //    this.sensor.DepthStream.Enable();
                //    this.sensor.SkeletonStream.Enable();
                //    this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                //    this.sensor.Start();
                //}
            }

        }

        void KinectSensorManager_KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> e)
        {
            var oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);

            var newSensor = (KinectSensor)e.NewValue;
            
            newSensor.ColorStream.Enable();
            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

            try
            {

                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                // TODO app conflict occurred 
            }
        }

        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(this.kinectSensorChooser.KinectSensorManager.KinectSensor);
        }

        private void StopKinect(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.AudioSource.Stop();
            }
        }
    }
}
