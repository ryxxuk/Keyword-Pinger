using System;
using System.Collections.Generic;
using System.Text;

namespace Keyword_Pinger.Models
{
    public static class Globals
    {
        public static Dictionary<string, KeywordInfo> LoadedKeywords = new();
        public static List<ulong> MonitoredChannels = new();
    }
}
