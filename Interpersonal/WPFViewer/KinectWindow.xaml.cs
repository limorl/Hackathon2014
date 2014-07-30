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
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.WpfViewers;
    using System.Windows.Media.Imaging;
    using System;
    using SpeakerTracking;
    using System.Diagnostics;
    using Interpersonal.WPFViewer;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

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

        private MeetingInfo currMeeting;
        private ISpeakerTracker tracker;
        private IUserDb userDb;
        private readonly KinectWindowViewModel viewModel;
        private PersonalDashboardWindow personalDashboard;

        
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

            //tracker.SpeakerChanged += (s, a) => Debug.WriteLine(String.Format("Change speeker from {0} to {1}", a.OldSpeaker != null ?  a.OldSpeaker.Index : -1, a.NewSpeaker.Index));

            //User data
            this.userDb = new UserDbMock();

            Binding sensorBinding = new Binding("KinectSensor");
            sensorBinding.Source = this;
            BindingOperations.SetBinding(this.viewModel.KinectSensorManager, KinectSensorManager.KinectSensorProperty, sensorBinding);

            // Attempt to turn on Skeleton Tracking for each Kinect Sensor
            this.viewModel.KinectSensorManager.SkeletonStreamEnabled = true;

            this.DataContext = this.viewModel;
            
            InitializeComponent();
            // initialize UI binding
            this.setupParticipants.Items.Clear();
            this.setupParticipants.ItemsSource = new ObservableCollection<User>(Configuration.Users.Values);
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
        #region Update User Db

        private void tracker_SpeakerVolumeChanged(object sender, SpeakerVolumeChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("Speaker volume changed: speakerId={0} from {1} to {2}", e.Speaker.Id, e.OldAudioLevel, e.NewAudioLevel));

            User speaker = null;
            if (TryGetMeetingAttendee(e.Speaker.Id, out speaker))
            {
                userDb.WriteSpeaker(speaker.Id, this.currMeeting.Id, DateTimeOffset.Now, e.NewAudioLevel);
            }
        }
        

        private void tracker_SpeakerInterrupted(object sender, SpeakerInterruptedEventArgs e)
        {
            Debug.WriteLine(string.Format("Speaker Interrupted: speakerId={0} interrupted to {1} (audioLevel={2}", 
                e.InterrupingSpeaker.Id, 
                e.InterruptedSpeaker.Id,
                e.AudioLevel));

            User speaker = null;
            if (TryGetMeetingAttendee(e.InterrupingSpeaker.Id, out speaker))
            {
                userDb.WriteInterruption(speaker.Id, this.currMeeting.Id, DateTimeOffset.Now, e.AudioLevel, e.InterruptedSpeaker.Id);
            }
        }

        private void tracker_SpeakerChanged(object sender, SpeakerChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("Speaker changed: oldSpeaker={0} newSpeaker={1} audioLevel = {2}",
                e.OldSpeaker.Id,
                e.NewSpeaker.Id,
                e.NewAudioLevel));

            User speaker = null;
            if (TryGetMeetingAttendee(e.NewSpeaker.Id, out speaker))
            {
                userDb.WriteSpeaker(speaker.Id, this.currMeeting.Id, DateTimeOffset.Now, e.NewAudioLevel);
            }
        }
        #endregion

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
                    kinectWindow.debugButtonImage.Source = UIUtils.GetImageSource(uri);	
                }
                else
                {
                    kinectWindow.settingsPanel.Visibility = Visibility.Collapsed;

                    var uri = new Uri(@"Images/arrow-left-round.png", UriKind.Relative);
                    kinectWindow.debugButtonImage.Source = UIUtils.GetImageSource(uri);
                }
            }

        }

        private bool TryGetMeetingAttendee(string userId, out User user)
        {
            User attendee = null;
            if (this.currMeeting != null && this.currMeeting.Attendees != null)
            {
                attendee = this.currMeeting.Attendees.Where((u) => u.Id == userId).FirstOrDefault();
            }

            user = attendee;
            return user != null;
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

        #region Meeting Setup
        private void meetingStart_Click(object sender, RoutedEventArgs e)
        {
            // if the meeting hasn't started yet
            if (this.currMeeting == null)
            {
                var meetingName = this.setupMeetingName.Text;
                var participants = GetMeetingParticipants();

                // create new meeting
                this.currMeeting = new MeetingInfo(meetingName, participants);
                
                // update the meeting panel
                this.meetingName.Content = this.currMeeting.Name;
                List<Image> userImages = new List<Image>(6){ this.imageUser0, this.imageUser1, this.imageUser2, this.imageUser3, this.imageUser4, this.imageUser5 };
                userImages.ForEach((i) => i.Source = null);

                for (int i = 0; i < participants.Count; i++)
                {
                    userImages[i].Source = UIUtils.GetImageSource(participants[i].ImageSource);	
                }

                // startSpeaker Tracking
                var userIds = participants.Select((u) => new UserIdentifier(u.Index, u.Id, u.Angle)).ToArray();
                this.tracker = new MockSpeakerTracker();
                tracker.SpeakerChanged += tracker_SpeakerChanged;
                tracker.SpeakerInterrupted += tracker_SpeakerInterrupted;
                tracker.SpeakerVolumeChanged += tracker_SpeakerVolumeChanged;

                this.meetingStart.Content = "stop";
            }
            else
            {
                // stop the current meeting
                this.currMeeting = null;

                // stop speaker tracking
                tracker.SpeakerChanged -= tracker_SpeakerChanged;
                tracker.SpeakerInterrupted -= tracker_SpeakerInterrupted;
                tracker.SpeakerVolumeChanged -= tracker_SpeakerVolumeChanged;
                this.tracker.Stop();
                this.tracker = null;

                // update UI
                this.meetingStart.Content = "start";
            }
        }

        private List<User> GetMeetingParticipants()
        {
            var participants = new List<User> { };

            for (int i = 0; i < this.setupParticipants.Items.Count; i++)
            {
                CheckBox checkBox = null;
                var participantItem = this.setupParticipants.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (participantItem != null)
                {
                    //get the item's template parent
                    ContentPresenter templateParent = UIUtils.GetFrameworkElementByName<ContentPresenter>(participantItem);

                    //get the DataTemplate that the CheckBox is in
                    DataTemplate dataTemplate = this.setupParticipants.ItemTemplate;
                    if (dataTemplate != null && templateParent != null)
                    {
                        checkBox = dataTemplate.FindName("checkBox", templateParent) as CheckBox;
                    }
                    if (checkBox != null && checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
                    {
                        // add user to the attendees
                        participants.Add((User)this.setupParticipants.Items.GetItemAt(i));
                    }
                }
            }

            return participants;
        }
        #endregion

        private void bttnParticipant_Click(object sender, RoutedEventArgs e)
        {
            ShowPersonalDashboard();
        }

        private void ShowPersonalDashboard()
        {
            if (this.personalDashboard != null)
            {
                this.personalDashboard.Close();
            }

            this.personalDashboard = new PersonalDashboardWindow();
            this.personalDashboard.Show();
            //this.Window.Activate();
        }
    }

    /// <summary>
    /// A ViewModel for a KinectWindow.
    /// </summary>
    public class KinectWindowViewModel : DependencyObject
    {
        public ObservableCollection<User> Users { get; set; }
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
