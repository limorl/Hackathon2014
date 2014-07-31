﻿namespace SpeakerTracking
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using System.Windows.Data;
    using Microsoft.Samples.Kinect.WpfViewers;
    using System.Windows.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        private ISpeakerTracker speakerTracker;

        public MainWindow()
        {
            InitializeComponent();
            int index = -1;
            var users = new UserIdentifier[] {
                new UserIdentifier(index++, "No One", 132),
                new UserIdentifier(index++, "deliak", 145),
              //  new UserIdentifier(index++, "huberte", 132),
                new UserIdentifier(index++, "limorl", 321),
              //  new UserIdentifier(index++, "yairg", 123),
                new UserIdentifier(index++, "yoramy", 123),

            };
            speakerTracker = new SpeakerVerificationUserTracker(users);

            speakerTracker.SpeakerChanged += speakerTracker_SpeakerChanged;
            speakerTracker.SpeakerInterrupted += speakerTracker_SpeakerInterrupted;

        }

        private void speakerTracker_SpeakerInterrupted(object sender, SpeakerInterruptedEventArgs e)
        {
            var tracker = sender as ISpeakerTracker;
            if (tracker != null)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            this.speakerInterruptedText.Text = "Interrupted by " + e.InterrupingSpeaker.Index;
                        }));
            }   
        }

        private void speakerTracker_SpeakerChanged(object sender, SpeakerChangedEventArgs e)
        {
            var tracker = sender as ISpeakerTracker;
            if (tracker != null)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            this.speakerIndexText.Text = e.NewSpeaker.Index.ToString();
                            this.speakerNameText.Text = e.NewSpeaker.Id;
                        }));
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
        }

        private void trackBttn_Click(object sender, RoutedEventArgs e)
        {
            if (speakerTracker.IsTracking())
            {
                speakerTracker.Stop();
                this.trackBttn.Content = "Track";
            }
            else
            {
                speakerTracker.Start();
                this.trackBttn.Content = "Stop Tracking";
            }
        }

      }

    
}
