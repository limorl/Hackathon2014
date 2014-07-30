
namespace SpeakerTracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Samples.Kinect.WpfViewers;

    public class KinectAudioSpeakerTracker : ISpeakerTracker
    {
        public KinectAudioSpeakerTracker( KinectSensorManager manager, IEnumerable<UserIdentifier> users)
        {
            manager.KinectSensorChanged += (a, b) =>
            {
                manager.KinectSensor.AudioSource.AutomaticGainControlEnabled = true;
                manager.KinectSensor.AudioSource.SoundSourceAngleChanged += (source, args) =>
                {
                    if (args.ConfidenceLevel >= 0.3)
                    {
                        var angle = args.Angle;
                        var matchingRange = users.OrderBy(u => Math.Abs(angle - u.Angle)).FirstOrDefault();
                        if (matchingRange != currentSpeaker)
                        {
                            var speakerChangedEventArgs = new SpeakerChangedEventArgs
                            {
                                OldSpeaker = this.currentSpeaker,
                                NewSpeaker = matchingRange
                            };
                            this.currentSpeaker = matchingRange;
                            SpeakerChanged.Invoke(this, speakerChangedEventArgs);
                        }
                    }
                };
            };
        }

        void AudioSource_BeamAngleChanged(object sender, Microsoft.Kinect.BeamAngleChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        public event EventHandler<SpeakerChangedEventArgs> SpeakerChanged;

        public event EventHandler<SpeakerVolumeChangedEventArgs> SpeakerVolumeChanged;

        public event EventHandler<SpeakerInterruptedEventArgs> SpeakerInterrupted;

        public event EventHandler<UpdateEventArgs> Update;

        public bool IsTracking()
        {
            return this._isTracking;
        }


        public SummaryData GetSummaryData()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            this._isTracking = true;
        }

        public void Stop()
        {
            this._isTracking = false;
        }

        private bool _isTracking = false;

        private AudioTracker _audioBase;
        private IEnumerable<UserIdentifier> _users;
        private UserIdentifier currentSpeaker = null;
    }
}
