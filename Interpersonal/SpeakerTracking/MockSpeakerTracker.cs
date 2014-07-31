

namespace SpeakerTracking
{
    using Microsoft.Kinect;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// A mock tracker that simulates 4 potential speakers, each is associated with a beam section of 22.5 degrees
    /// It simulates a meeting where each user speaks for a period of time, then with some probability it increase the voice, or being interrupted
    /// </summary>
    public class MockSpeakerTracker : ISpeakerTracker
    {
        private const double MinSpeakerAngle = -45;
        private const double MaxSpeakerAngle = 45;
        
        private const int UpdateIntervalMillis = 500;
        private const int NoSpeaker = -1;
        private const double ProbabilityToContinueSpeaking = 0.9;
        private const double ProbabilityToBeInterrupted = 0.3;
        private const double ProbabilityToHaveSpeaker = 0.8;

        private List<UserIdentifier> userIds;
        private List<UserData> users;
        private bool usersLock;
        private int currSpeakerIndex;
        private DateTime lastUpdateTime;
        private Thread simulationThread;
        private Random random;       
        private int totalMillis;
        private bool isTracking;

        public event EventHandler<SpeakerChangedEventArgs> SpeakerChanged;
        public event EventHandler<SpeakerVolumeChangedEventArgs> SpeakerVolumeChanged;
        public event EventHandler<SpeakerInterruptedEventArgs> SpeakerInterrupted;
        public event EventHandler<UpdateEventArgs> Update;


        /// <summary>
        /// Initializes a new instance of the FaceTracker class from a reference of the Kinect device.
        /// <param name="sensor">Reference to kinect sensor instance</param>
        /// </summary>
        public MockSpeakerTracker()
        {
            currSpeakerIndex = -1;
            lastUpdateTime = DateTime.MinValue;
            simulationThread = new Thread(Simulate);
            random = new Random();
            totalMillis = 0;
            isTracking = false;

            // initialize with empty events handlers
            SpeakerChanged += OnSpeakerChanged;
            SpeakerVolumeChanged += OnSpeakerVolumeChanged;
            SpeakerInterrupted += OnSpeakerInterrupted;
            Update += OnUpdate;

            InitializeMockUsers();
        }

        public UserIdentifier CurrentSpeaker
        {
            get
            {
                if (this.currSpeakerIndex == -1)
                {
                    return null;
                }
                return this.users[this.currSpeakerIndex].User;
            }
        }

        public UserData CurrentSpeakerData
        {
            get
            {
                if (this.currSpeakerIndex == -1)
                {
                    return null;
                }
                return this.users[this.currSpeakerIndex];
            }
        }

        public bool IsTracking()
        {
            return this.isTracking;
        }

        private void InitializeMockUsers()
        {
            this.userIds = new List<UserIdentifier>
            {
                new UserIdentifier(0, "deliak", -15),
                new UserIdentifier(1, "yoramy", 45),
                new UserIdentifier(2, "limorl", 15),
                new UserIdentifier(3, "hubert", 45)
            };

            var userCount = userIds.Count;

            this.users = new List<UserData>();

            var userAngle = MinSpeakerAngle;
            var deltaAngle = (MaxSpeakerAngle - MinSpeakerAngle) / userCount;
        
            for (int i = 0; i < userCount; i++)
            {
                // TODO: add suport in getting user history and merge
                var user = new UserIdentifier(i, userIds.ElementAt(i).Id, userIds.ElementAt(i).Angle);
                var userData = new UserData(user);
                this.users.Add(userData);

                // update user angle
                userAngle += deltaAngle;
            }
        }

        private void Simulate()
        {
            if(this.simulationThread == null )
            {
                return;
            }

            while (isTracking)
            {
                totalMillis += UpdateIntervalMillis;

                // simulate speaker was detected with some probability
                var speakerDetected = SimulateSpeakerDetectedWithProbability();
                var speakerInterrupted = SimulateSpeakerChangedWithProbability();

                if (speakerDetected)
                {
                    UpdateSpeakerData();
                }

                Thread.Sleep(UpdateIntervalMillis);
            }
        }

        private bool SimulateSpeakerDetectedWithProbability()
        {
            var newSpeakerIndex = this.currSpeakerIndex; ;
            if (random.NextDouble() < ProbabilityToHaveSpeaker)
            {
                // if no speaker, find a random one
                if (this.currSpeakerIndex == NoSpeaker)
                {
                    newSpeakerIndex = GetRandomSpeaker();
                }
                if (this.currSpeakerIndex != newSpeakerIndex)
                {
                    var eventArgs = new SpeakerChangedEventArgs
                    {
                        OldSpeaker = this.currSpeakerIndex == -1 ? null : GetUserIdentifier(this.currSpeakerIndex),
                        NewSpeaker = GetUserIdentifier(newSpeakerIndex)
                    };

                    this.currSpeakerIndex = newSpeakerIndex;
                    SpeakerChanged(this, eventArgs);

                    return true;
                }
             
                return true;
            }
            
            // no speaker was detected
            this.currSpeakerIndex = -1;
            return false;
        }

        private bool SimulateSpeakerChangedWithProbability()
        {
            // simulate speaker has changed or current speaker has continuted to talk
            var newSpeakerIndex = GetRandomSpeaker();
            if (this.currSpeakerIndex != newSpeakerIndex)
            {
                var eventArgs = new SpeakerChangedEventArgs
                {
                    OldSpeaker = this.currSpeakerIndex == -1 ? null : GetUserIdentifier(this.currSpeakerIndex),
                    NewSpeaker = GetUserIdentifier(newSpeakerIndex)
                };

                this.currSpeakerIndex = newSpeakerIndex;
                SpeakerChanged(this, eventArgs);

                return true;
            }

            return false;
        }

        private int GetRandomSpeaker()
        {
            if (this.currSpeakerIndex == NoSpeaker)
            {
                return random.Next(this.userIds.Count);
            }
            
            if (random.NextDouble() < ProbabilityToContinueSpeaking)
            {
                // continue with the same speaker                     
                return this.currSpeakerIndex;
            }

            // else, return a new random speaker
            return random.Next(this.userIds.Count); 
        }

        private bool SimulateSpeakerInterruptedWithProbability()
        {
            if (random.NextDouble() < ProbabilityToBeInterrupted)
            {
                // get a random interrupting speaker and if it's the same as the current one, set it to the next user (cyclic)
                var interruptingSpeakerId = random.Next(this.userIds.Count);
                if (interruptingSpeakerId == this.currSpeakerIndex)
                {
                    interruptingSpeakerId = (interruptingSpeakerId + 1) % this.userIds.Count;
                }

                var eventArgs = new SpeakerInterruptedEventArgs
                {
                    InterruptedSpeaker = GetUserIdentifier(this.currSpeakerIndex),
                    InterrupingSpeaker = GetUserIdentifier(interruptingSpeakerId)
                };
                SpeakerInterrupted(this, eventArgs);
                return true;
            }

            return false;
        }

        private void UpdateSpeakerData()
        {
            // add speaking event to the current user
            CurrentSpeakerData.AddSpeakingEvent(50 + random.Next(40));
        }

        public SummaryData GetSummaryData()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            if (isTracking)
            {
                throw new InvalidOperationException("Already started tracking");
            }

            this.users.ForEach(u => u.StartSession());
            this.simulationThread.Start();
            isTracking = true;
        }

        public void Stop()
        {
            if (!isTracking)
            {
                throw new InvalidOperationException("Hasn't started tracking yet");
            }

            this.users.ForEach(u => u.EndSession());
            this.simulationThread.Suspend();
            this.isTracking = false;
        }

        private UserIdentifier GetUserIdentifier(int index)
        {
            return this.users[index].User;
        }

        #region Event Handlers to update User Data
        // dummy implementation, to avoid checking for null event handler
        private void OnSpeakerChanged(object sender, SpeakerChangedEventArgs e)
        {
           
        }

        // dummy implementation, to avoid checking for null event handler
        private void OnSpeakerVolumeChanged(object sender, SpeakerVolumeChangedEventArgs e)
        {
        }

        // dummy implementation, to avoid checking for null event handler
        private void OnSpeakerInterrupted(object sender, SpeakerInterruptedEventArgs e)
        {
        }

        // dummy implementation, to avoid checking for null event handler
        private void OnUpdate(object sender, UpdateEventArgs e)
        {
             var tracker = (MockSpeakerTracker)sender;
             tracker.users[tracker.currSpeakerIndex].AddSpeakingEvent(e.AudioLevel);
        }
        #endregion 
    

        
    }
}