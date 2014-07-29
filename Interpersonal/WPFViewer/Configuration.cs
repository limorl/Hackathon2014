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
            { "deliak", new User("deliak", "Delia", @"Images/deliak.png") },
            { "huberte", new User("huberte", "Hubert", @"Images/huberte.png") },
            { "limorl", new User("limorl", "Limor", @"Images/limorl.png") },
            { "yairg", new User("yairg", "Yair", @"Images/yairg.png") },
            { "yoramy", new User("yoramy", "Yoram", @"Images/yoramy.png") },
        };
    }

   
}
