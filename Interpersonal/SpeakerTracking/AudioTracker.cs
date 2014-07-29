
namespace SpeakerTracking
{
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.WpfViewers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;

    class AudioTracker : KinectTrackerBase
    {
        private Stream audioStream;
        private KinectSensorManager sensorManager;

        public AudioTracker(KinectSensorManager sensorManager)
        {
            if (sensorManager == null)
            {
                throw new ArgumentNullException("sensorManager");
            }

            this.sensorManager = sensorManager;
            sensorManager.KinectSensorChanged += OnKinectSensorChanged;
            sensorManager.KinectStatusChanged += OnKinectStatusChanged;
            sensorManager.AudioWasResetBySkeletonEngine += OnAudioWasResetBySkeletonEngine;

            sensorManager.KinectSensor.AudioSource.BeamAngleChanged += AudioSourceBeamChanged;
            sensorManager.KinectSensor.AudioSource.SoundSourceAngleChanged += AudioSourceSoundSourceAngleChanged;

        }
        
        public double BeamAngle { get; private set; }
       
        public double SourceAngle { get; private set; }

        public double Confidence { get; private set; } 

        protected override void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }

            if ((null != args.OldValue) && (null != args.OldValue.AudioSource))
            {
                // remove old handlers
                args.OldValue.AudioSource.BeamAngleChanged -= this.AudioSourceBeamChanged;
                args.OldValue.AudioSource.SoundSourceAngleChanged -= this.AudioSourceSoundSourceAngleChanged;
                
                if (null != this.audioStream)
                {
                    this.audioStream.Close();
                }

                args.OldValue.AudioSource.Stop();
            }

            if ((null != args.NewValue) && (null != args.NewValue.AudioSource))
            {
                // add new handlers
                args.NewValue.AudioSource.BeamAngleChanged += this.AudioSourceBeamChanged;
                args.NewValue.AudioSource.SoundSourceAngleChanged += this.AudioSourceSoundSourceAngleChanged;

                this.EnsureAudio(args.NewValue, args.NewValue.Status);
            }
        }

        protected override void OnKinectStatusChanged(object sender, KinectSensorManagerEventArgs<KinectStatus> args)
        {
            if ((null != this.KinectSensorManager) && (null != this.KinectSensorManager.KinectSensor) && (null != this.KinectSensorManager.KinectSensor.AudioSource))
            {
                this.EnsureAudio(this.KinectSensorManager.KinectSensor, this.KinectSensorManager.KinectSensor.Status);
            }            
        }

        protected override void OnAudioWasResetBySkeletonEngine(object sender, EventArgs args)
        {
            if ((null != this.KinectSensorManager) && (null != this.KinectSensorManager.KinectSensor) && (null != this.KinectSensorManager.KinectSensor.AudioSource))
            {
                this.EnsureAudio(this.KinectSensorManager.KinectSensor, this.KinectSensorManager.KinectSensor.Status);
            }
        }

        private void EnsureAudio(KinectSensor sensor, KinectStatus status)
        {
            if ((null != sensor) && (KinectStatus.Connected == status) && sensor.IsRunning)
            {
                this.audioStream = sensor.AudioSource.Start();
            }
        }

        private void AudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            // Set width of mark based on confidence
            this.Confidence = (int)Math.Round(e.ConfidenceLevel * 100);

            // Move indicator
            this.SourceAngle = e.Angle;
        }

        private void AudioSourceBeamChanged(object sender, BeamAngleChangedEventArgs e)
        {
            // Move our indicator
            this.BeamAngle = e.Angle;
        }
    }
}
