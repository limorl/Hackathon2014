using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerTracking
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Samples.Kinect.WpfViewers;
    using System.Windows;
    using Microsoft.Kinect;
    using System.IO;
    using System.Threading;

    // Attribute simple SpeakerTracking based on users angle
    public class SimpleKinectAudioSpeakerTracking : ISpeakerTracker
    {
        public SimpleKinectAudioSpeakerTracking(KinectSensorManager manager,  Func<IEnumerable<UserIdentifier>> users)
        {
             manager.KinectSensorChanged += (a, b) =>
            {
                // Start streaming audio!
                this.audioStream = manager.KinectSensor.AudioSource.Start();

                // Use a separate thread for capturing audio because audio stream read operations
                // will block, and we don't want to block main UI thread.
                this.reading = true;
                this.readingThread = new Thread(AudioReadingThread);
                this.readingThread.Start();

                manager.KinectSensor.AudioSource.AutomaticGainControlEnabled = true;
                manager.KinectSensor.AudioSource.SoundSourceAngleChanged += (sender, e) =>
                    {
                        if (e.ConfidenceLevel >= 0.3)
                        {
                            var angle = e.Angle;
                            var matchingRange = users().OrderBy(u => Math.Abs(angle - u.Angle)).FirstOrDefault();
                            if (!UserIdentifier.Equals(matchingRange, currentSpeaker))
                            {
                                // if there wasn't silence --> than the previous one was interrupted
                                if (currentSpeaker != null && lastAudioLevel > 50)
                                {
                                    var speakerInterruptedEventArgs = new SpeakerInterruptedEventArgs
                                    {
                                        InterrupingSpeaker = matchingRange,
                                        InterruptedSpeaker = currentSpeaker,
                                        AudioLevel = this.lastAudioLevel
                                    };
                                    if(SpeakerInterrupted != null)
                                    {
                                        SpeakerInterrupted.Invoke(this, speakerInterruptedEventArgs);
                                    }
                                }

                                // the speaker has changed 
                                var speakerChangedEventArgs = new SpeakerChangedEventArgs
                                {
                                    OldSpeaker = this.currentSpeaker,
                                    NewSpeaker = matchingRange,
                                    //NewAudioLevel = this.energy
                                };
                                this.currentSpeaker = matchingRange;
                                if (SpeakerChanged != null)
                                {
                                    SpeakerChanged.Invoke(this, speakerChangedEventArgs);
                                }
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
        private int lastAudioLevel;
        private IEnumerable<UserIdentifier> _users;
        private UserIdentifier currentSpeaker = null;

        #region Audio Sensing
        /// <summary>
        /// Number of milliseconds between each read of audio data from the stream.
        /// Faster polling (few tens of ms) ensures a smoother audio stream visualization.
        /// </summary>
        private const int AudioPollingInterval = 50;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        private const int BytesPerSample = 2;

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;

        /// <summary>
        /// Width of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapWidth = 780;

        /// <summary>
        /// Height of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapHeight = 195;

        /// <summary>
        /// Rectangle representing the entire energy bitmap area. Used when drawing background
        /// for energy visualization.
        /// </summary>
        private readonly Int32Rect fullEnergyRect = new Int32Rect(0, 0, EnergyBitmapWidth, EnergyBitmapHeight);

        /// <summary>
        /// Array of background-color pixels corresponding to an area equal to the size of whole energy bitmap.
        /// </summary>
        private readonly byte[] backgroundPixels = new byte[EnergyBitmapWidth * EnergyBitmapHeight];

        /// <summary>
        /// Buffer used to hold audio data read from audio stream.
        /// </summary>
        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        /// <summary>
        /// Buffer used to store audio stream energy data as we read audio.
        /// 
        /// We store 25% more energy values than we strictly need for visualization to allow for a smoother
        /// stream animation effect, since rendering happens on a different schedule with respect to audio
        /// capture.
        /// </summary>
        private readonly double[] energy = new double[(uint)(EnergyBitmapWidth * 1.25)];

        /// <summary>
        /// Object for locking energy buffer to synchronize threads.
        /// </summary>
        private readonly object energyLock = new object();

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Stream of audio being captured by Kinect sensor.
        /// </summary>
        private Stream audioStream;

        /// <summary>
        /// <code>true</code> if audio is currently being read from Kinect stream, <code>false</code> otherwise.
        /// </summary>
        private bool reading;

        /// <summary>
        /// Thread that is reading audio from Kinect stream.
        /// </summary>
        private Thread readingThread;

        /// <summary>
        /// Array of foreground-color pixels corresponding to a line as long as the energy bitmap is tall.
        /// This gets re-used while constructing the energy visualization.
        /// </summary>
        private byte[] foregroundPixels;

        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private double accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// Index of next element available in audio energy buffer.
        /// </summary>
        private int energyIndex;

        /// <summary>
        /// Number of newly calculated audio stream energy values that have not yet been
        /// displayed.
        /// </summary>
        private int newEnergyAvailable;

        /// <summary>
        /// Error between time slice we wanted to display and time slice that we ended up
        /// displaying, given that we have to display in integer pixels.
        /// </summary>
        private double energyError;

        /// <summary>
        /// Last time energy visualization was rendered to screen.
        /// </summary>
        private DateTime? lastEnergyRefreshTime;

        /// <summary>
        /// Index of first energy element that has never (yet) been displayed to screen.
        /// </summary>
        private int energyRefreshIndex;

        /// <summary>
        /// Handles event triggered when sound source angle changes.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void AudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// Handles polling audio stream and updating visualization every tick.
        /// </summary>
        private void AudioReadingThread()
        {
            // Bottom portion of computed energy signal that will be discarded as noise.
            // Only portion of signal above noise floor will be displayed.
            const double EnergyNoiseFloor = 0.2;

            while (this.reading)
            {
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);

                // Calculate energy corresponding to captured audio.
                // In a computationally intensive application, do all the processing like
                // computing energy, filtering, etc. in a separate thread.
                lock (this.energyLock)
                {
                    for (int i = 0; i < readCount; i += 2)
                    {
                        // compute the sum of squares of audio samples that will get accumulated
                        // into a single energy value.
                        short audioSample = BitConverter.ToInt16(audioBuffer, i);
                        this.accumulatedSquareSum += audioSample * audioSample;
                        ++this.accumulatedSampleCount;

                        if (this.accumulatedSampleCount < SamplesPerColumn)
                        {
                            continue;
                        }

                        // Each energy value will represent the logarithm of the mean of the
                        // sum of squares of a group of audio samples.
                        double meanSquare = this.accumulatedSquareSum / SamplesPerColumn;
                        double amplitude = Math.Log(meanSquare) / Math.Log(int.MaxValue);

                        // Renormalize signal above noise floor to [0,1] range.
                        this.energy[this.energyIndex] = Math.Max(0, amplitude - EnergyNoiseFloor) / (1 - EnergyNoiseFloor);
                        this.energyIndex = (this.energyIndex + 1) % this.energy.Length;

                        this.accumulatedSquareSum = 0;
                        this.accumulatedSampleCount = 0;
                        ++this.newEnergyAvailable;
                    }
                }
            }
        }
        #endregion
    }
}
