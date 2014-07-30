using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Samples.Kinect.WpfViewers;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SpeakerTracking
{
    public class SpeakerVerificationUserTracker : ISpeakerTracker
    {
        private NamedPipeClientStream client;
        static readonly string pipeName = @"\\.\pipe\interpersonal";
        public SpeakerVerificationUserTracker(IEnumerable<UserIdentifier> users)
        {
            client = new NamedPipeClientStream("interpersonal");
            client.Connect();
            this._users = users;
            Task.Factory.StartNew( () =>
                {
                    var reader = new StreamReader(client);
                    while (true)
                    {
                        // The line format is 
                        // a {TimeStamp} {Speeker ID} {AudioLevel} 
                        var line = reader.ReadLine();
                        var parsedData = line.Split(' ');
                        if (String.Equals(parsedData[0],"a"))
                        {
                            Debug.WriteLine("Invalid line " + line);
                        }
                        var speakerId = int.Parse(parsedData[2]);
                        if (this.currentSpeaker.Index != speakerId)
                        {
                            // 
                            // The user has changed lets notify 
                            //
                            var newSpeaker = this._users.Where(u => u.Index == speakerId).FirstOrDefault();
                            if (newSpeaker == default(UserIdentifier))
                            {
                                Debug.WriteLine("Invalid speaker Id raised");
                            }

                            var speakerChangedEventArg = new SpeakerChangedEventArgs { OldSpeaker = this.currentSpeaker, NewSpeaker = newSpeaker };
                            this.currentSpeaker = newSpeaker;
                            SpeakerChanged.Invoke(this, speakerChangedEventArg);
                        }
                    }
                });
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

        private IEnumerable<UserIdentifier> _users;
        private UserIdentifier currentSpeaker = null;
    }
}
