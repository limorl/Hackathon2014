using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpersonal.WPFViewer
{
    class MeetingInfo
    {
        // random for generating unique ids
        private static Random random = new Random();

        public string Id { get; private set; }
        public string Name { get; private set; }
        public IEnumerable<User> Attendees{ get; private set; }

        public MeetingInfo(string name, IEnumerable<User> attendees)
        {
            ValidateMeetingInfo(name, attendees);
            Id = GenerateMeetingId(name);
            Name = name;
            Attendees = attendees;
        }

        private static string GenerateMeetingId(string name)
        {
 	        return name.Replace(' ', '-').Replace('\t', '-') + random.Next(25);
        }

        private void ValidateMeetingInfo(string name, IEnumerable<User> attendees)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (attendees == null || !attendees.Any())
            {
                throw new ArgumentException("No attendees");
            }
        }
    }
}
