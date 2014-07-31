

namespace Interpersonal.WPFViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class User
    {
        // the index of the user as it is in the speach training model configuration
        public int Index { get; set; }

        // use alias
        public string Id { get; private set; }

        public string Name { get; private set; }
        public string ImageSource { get; private set; }

        // should be dependency property
        public int Angle { get; set; }

        public User(int index, string id, string name, string imageRelativePath, int angle = 0)
        {
            //  TODO: validate input 
            Index = index;
            Id = id;
            Name = name;
            ImageSource = imageRelativePath; // new Uri(imageRelativePath, UriKind.Relative);
            Angle = angle;
        }
    }
}
