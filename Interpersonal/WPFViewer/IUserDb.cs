using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Interpersonal.WPFViewer
{
    interface IUserDb
    {
        // write interface
        void WriteSpeaker(string userId, string meetingId, DateTimeOffset timestampUtc, int audioLevel);

        void WriteInterruption(string userId, string meetingId, DateTimeOffset timestampUtc, int audioLevel, string interruptedUserId);


        // read interface 
        // if no meeting Id, returns all meeting
        UserSummary ReadUserSummary(string userId, string meetingId = null); 
    }

    public class UserSummary { }
    // Mock User Db
    public class UserDbMock : IUserDb
    {
        public void WriteSpeaker(string userId, string meetingId, DateTimeOffset timestampUtc, int audioLevel)
        {
            Debug.WriteLine(String.Format("UserDb: Speaker [{0}] userId={1} meetingId={2} audioLevel={3}", 
                timestampUtc.ToString(), 
                userId != null ? userId : "None", 
                meetingId, audioLevel));
        }

        public void WriteInterruption(string userId, string meetingId, DateTimeOffset timestampUtc, int audioLevel, string interruptedUserId)
        {
            Debug.WriteLine(String.Format("UserDb: Interrupting [{0}] userId={1} meetingId={2} audioLevel={3} interrupted={4}",
                timestampUtc.ToString(),
                userId != null ? userId : "None",
                meetingId, audioLevel,
                interruptedUserId));
        }

        public UserSummary ReadUserSummary(string userId, string meetingId = null)
        {
            return new UserSummary();
        }
    }
}
