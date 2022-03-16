using System;
using System.Collections.Generic;

namespace Keyword_Pinger.Models
{
    public class KeywordInfo
    {
        public DateTime LastPinged { get; set; }
        public List<ulong> UserId = new();
        public List<ulong> RoleId = new();
        public List<ulong> ChannelId = new();
    }
}
