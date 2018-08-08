using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ArkCommander
{
    public static class StaticVars
    {
        public static bool allowShowHideNow = false;

        public static ArkDino[] dinos;

        public static ChromiumWebBrowser browser;

        public static Process arkWindow;
    }

    [DataContract]
    public class ArkDino
    {
        [DataMember]
        public string name;
        [DataMember]
        public string img;
        [DataMember]
        public string id;
        [DataMember]
        public string url;
    }
}
