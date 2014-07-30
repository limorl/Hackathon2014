//------------------------------------------------------------------------------
// <copyright file="KinectWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectExplorer
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.WpfViewers;
    using System.Windows.Media.Imaging;
    using System;
    using SpeakerTracking;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for KinectWindow.xaml.
    /// </summary>
    public partial class KinectWindow : Window
    {
        public static readonly DependencyProperty KinectSensorProperty =
            DependencyProperty.Register(
                "KinectSensor",
                typeof(KinectSensor),
                typeof(KinectWindow),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SettingsVisibilityProperty =
           DependencyProperty.Register(
               "SettingsVisibility",
               typeof(bool),
               typeof(KinectWindow),
               new PropertyMetadata(true, SettingsVisibility_Changed));             
               // new PropertyMetadata(null));     

        private readonly KinectWindowViewModel viewModel;
        /// <summary>
        /// Initializes a new instance of the KinectWindow class, which provides access to many KinectSensor settings
        /// and output visualization.
        /// </summary>
        public KinectWindow()
        {
            this.viewModel = new KinectWindowViewModel();

            // The KinectSensorManager class is a wrapper for a KinectSensor that adds
            // state logic and property change/binding/etc support, and is the data model
            // for KinectDiagnosticViewer.
            this.viewModel.KinectSensorManager = new KinectSensorManager();
            var users = new UserIdentifier[] {
                new UserIdentifier(index:1, seatAngle: 47.5),
                new UserIdentifier(index:2, seatAngle:-23)
            };

            Binding sensorBinding = new Binding("KinectSensor");
            sensorBinding.Source = this;
            BindingOperations.SetBinding(this.viewModel.KinectSensorManager, KinectSensorManager.KinectSensorProperty, sensorBinding);

            // Attempt to turn on Skeleton Tracking for each Kinect Sensor
            this.viewModel.KinectSensorManager.SkeletonStreamEnabled = true;

            this.DataContext = this.viewModel;
            
            InitializeComponent();
        }

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)GetValue(KinectSensorProperty); }
            set { SetValue(KinectSensorProperty, value); }
        }

        public bool SettingsVisibility
        {
            get { return (bool)GetValue(SettingsVisibilityProperty); }
            set { SetValue(SettingsVisibilityProperty, value); }
        }

        private static void SettingsVisibility_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var kinectWindow = (KinectWindow)sender;

            if (kinectWindow != null)
            {
                var visibility = (bool)args.NewValue;

                if (visibility)
                {
                    kinectWindow.settingsPanel.Visibility = Visibility.Visible;

                    var uri = new Uri(@"Images/arrow-right-round.png", UriKind.Relative);
                    kinectWindow.debugButtonImage.Source = GetImageSource(uri);	
                }
                else
                {
                    kinectWindow.settingsPanel.Visibility = Visibility.Collapsed;

                    var uri = new Uri(@"Images/arrow-left-round.png", UriKind.Relative);
                    kinectWindow.debugButtonImage.Source = GetImageSource(uri);
                }
            }

        }

        private static BitmapImage GetImageSource(Uri relativeUri)
        {
            var newImage = new BitmapImage();

            newImage.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache;
            newImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.None;
            Uri urisource = relativeUri;
            newImage.BeginInit();
            newImage.UriSource = urisource;
            newImage.EndInit();

            return newImage;
        }

        public void StatusChanged(KinectStatus status)
        {
            this.viewModel.KinectSensorManager.KinectSensorStatus = status;
        }

        private void Swap_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Grid colorFrom = null;
            Grid depthFrom = null;

            if (this.MainViewerHost.Children.Contains(this.ColorVis))
            {
                colorFrom = this.MainViewerHost;
                depthFrom = this.SideViewerHost;
            }
            else
            {
                colorFrom = this.SideViewerHost;
                depthFrom = this.MainViewerHost;
            }

            colorFrom.Children.Remove(this.ColorVis);
            depthFrom.Children.Remove(this.DepthVis);
            colorFrom.Children.Insert(0, this.DepthVis);
            depthFrom.Children.Insert(0, this.ColorVis);
        }

        private void radioButtonMonitorMode_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void radioButtonSimulationMode_Checked(object sender, RoutedEventArgs e)
        {

        }

     
        private void KinectSettings_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            this.SettingsVisibility = !this.SettingsVisibility;
        }

       
    }

    /// <summary>
    /// A ViewModel for a KinectWindow.
    /// </summary>
    public class KinectWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(KinectWindowViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DepthTreatmentProperty =
            DependencyProperty.Register(
                "DepthTreatment",
                typeof(KinectDepthTreatment),
                typeof(KinectWindowViewModel),
                new PropertyMetadata(KinectDepthTreatment.ClampUnreliableDepths));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        public KinectDepthTreatment DepthTreatment
        {
            get { return (KinectDepthTreatment)GetValue(DepthTreatmentProperty); }
            set { SetValue(DepthTreatmentProperty, value); }
        }
    }

    /// <summary>
    /// The Command to swap the viewer in the main panel with the viewer in the side panel.
    /// </summary>
    public class KinectWindowsViewerSwapCommand : RoutedCommand
    {  
    }
}
