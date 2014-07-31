using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpersonal.WPFViewer
{
    public class Configuration
    {
        public static IDictionary<string, User> Users = new Dictionary<string, User>
        {
            { "deliak", new User(0, "deliak", "Delia", @"Images/deliak.jpg") },
            { "huberte", new User(1, "huberte", "Hubert", @"Images/huberte.jpg") },
            { "limorl", new User(2, "limorl", "Limor", @"Images/limorl.jpg") },
            { "yairg", new User(3, "yairg", "Yair", @"Images/yairg.jpg") },
            { "yoramy", new User(4, "yoramy", "Yoram", @"Images/yoramy.jpg") },
        };
    }

   
}
