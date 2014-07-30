using System;
using System.Collections.Generic;
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
}
