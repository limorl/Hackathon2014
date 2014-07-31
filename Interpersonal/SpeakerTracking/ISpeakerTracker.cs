using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerTracking
{
    public interface ISpeakerTracker
    {
        event EventHandler<SpeakerChangedEventArgs> SpeakerChanged;

        event EventHandler<SpeakerVolumeChangedEventArgs> SpeakerVolumeChanged;

        event EventHandler<SpeakerInterruptedEventArgs> SpeakerInterrupted;

        bool IsTracking();

        /// <summary>
        /// Called every time interval and updates the status of the speakers
        /// </summary>
        event EventHandler<UpdateEventArgs> Update;

        SummaryData GetSummaryData();

        // start tracking session
        void Start();

        // end tracking session
        void Stop();
    }

    #region event args
    public class SpeakerChangedEventArgs : EventArgs
    {
        public UserIdentifier OldSpeaker { get; set; }

        public UserIdentifier NewSpeaker { get; set; }

        public int NewAudioLevel { get; set; }
    }

    public class SpeakerVolumeChangedEventArgs : EventArgs
    {
        public UserIdentifier Speaker { get; set; }

        public int OldAudioLevel { get; set; }

        public int NewAudioLevel { get; set; }
    }

    public class SpeakerInterruptedEventArgs : EventArgs
    {
        public UserIdentifier InterruptedSpeaker { get; set; }

        public UserIdentifier InterrupingSpeaker { get; set; }

        public int AudioLevel { get; set; }
    }

    public class UpdateEventArgs : EventArgs
    {
        public UserIdentifier TrackedUser { get; set; }

        public int AudioLevel { get; set; }
    }
    #endregion 

#region Result Data

    public class UserIdentifier
    {
        // This is the index in the lst configuration file
        public int Index { get; private set; }

        // the alias
        public string Id { get; private set; }

        public double Angle { get; private set; }

        public UserIdentifier(int index, string id, double seatAngle)
        {
            this.Index = index;
            this.Id = id;
            this.Angle = seatAngle;
        }

        public static bool Equals(UserIdentifier user1, UserIdentifier user2)
        {
            if (user1 != null && user2 != null)
            {
                return user1.Id == user2.Id;
            }

            if (user1 == null && user2 == null)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is UserIdentifier))
                return false;
            else
                return this.Id == ((UserIdentifier)obj).Id;
        }
    }

    public class UserData
    {
        private List<SessionData> sessions;
        private SessionData currSession;

        public UserData(UserIdentifier user)
        {
            User = user;
            this.sessions = new List<SessionData>();
            this.currSession = null;            
        }

        public readonly UserIdentifier User;

        public void StartSession()
        {
            if (currSession != null)
            {
                throw new InvalidOperationException("Session has already started");
            }

            currSession = new SessionData();
            currSession.Start();
        }

        public void EndSession()
        {
            if (currSession != null)
            {
             //   throw new InvalidOperationException("Cannot end session before it started");
            }

            currSession.End();
            this.sessions.Add(currSession);
            this.currSession = null;
        }

        public void AddSpeakingEvent(int audioLevel)
        {
            this.currSession.AddSpeakingEvent(audioLevel);
        }

        public void AddInterruptingEvent(UserIdentifier otherUser)
        {
            this.currSession.AddInterruptingEvent(otherUser);
        }

        public void AddInterruptedEvent(UserIdentifier otherUser)
        {
            this.currSession.AddInterruptedEvent(otherUser);
        }
    }

    public class SessionData
    {
        private bool isStarted;

        private List<SpeakingSessionEvent> speakingEvents;
        private List<InterruptedSessionEvent> interruptedEvents;
        private List<InterruptingSessionEvent> interruptingEvents;

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public SessionData()
        {
            isStarted = false;
            speakingEvents = new List<SpeakingSessionEvent>();
            interruptedEvents = new List<InterruptedSessionEvent>();
            interruptingEvents = new List<InterruptingSessionEvent>();
        }

        public int TotalSpeakingTimeInSeconds { get; private set; }

        public void Start()
        {
            if (isStarted)
            {
                throw new InvalidOperationException("session has already started");
            }

            StartTime = DateTime.UtcNow;
            isStarted = true;
        }

        public void End()
        {
            if (!isStarted)
            {
                throw new InvalidOperationException("Cannot end session before it started");
            }

            EndTime = DateTime.UtcNow;
            isStarted = false;
        }

        public void AddSpeakingEvent(int audioLevel)
        {
            this.speakingEvents.Add(new SpeakingSessionEvent(audioLevel));
        }

        public void AddInterruptingEvent(UserIdentifier otherUser)
        {
            this.interruptingEvents.Add(new InterruptingSessionEvent(otherUser));
        }

        public void AddInterruptedEvent(UserIdentifier otherUser)
        {
            this.interruptedEvents.Add(new InterruptedSessionEvent(otherUser));
        }
    }

    public abstract class SessionEventBase 
    {
        public DateTime Timestamp { get; private set; }

        protected SessionEventBase()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    public class InterruptingSessionEvent : SessionEventBase
    {
        public UserIdentifier OtherUser { get; private set; }

        public InterruptingSessionEvent(UserIdentifier otherUser)
        {
            // TODO (low) clone
            OtherUser = otherUser;
        }
    }

    public class InterruptedSessionEvent : SessionEventBase
    {
        public UserIdentifier OtherUser { get; private set; }

        public InterruptedSessionEvent(UserIdentifier otherUser)
        {
            // TODO (low) clone
            OtherUser = otherUser;
        }
    }

    public class SpeakingSessionEvent : SessionEventBase
    {
        public int AudioLevel { get; private set; }

        public SpeakingSessionEvent(int audioLevel)
        {
            AudioLevel = audioLevel;
        }

    }

    public class SummaryData
    { }
#endregion 
}
