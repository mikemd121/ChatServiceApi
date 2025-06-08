using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServiceApi
{
   public class ChatSession
    {
        public string Username { get; set; } = string.Empty;
        public int MissedPollCount { get; set; } = 0;
        public string State { get; set; } = "PENDING";
        public string? AssignedAgentName { get; set; }

        public bool IsInactive;

        public bool IsOverflow { get; set; }
    }
}
