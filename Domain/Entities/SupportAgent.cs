using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServiceApi
{
    public class SupportAgent
    {
        public string Name { get; set; } = string.Empty;
        public AgentSeniority Seniority { get; set; }
        public List<string> ActiveSessions { get; set; } = new();
        public bool IsOverflow { get; set; }

        public int GetMaxCapacity()
        {
            return (int)(10 * AgentEfficiency.Get(Seniority));
        }

        public int GetAvailableSlots()
        {
            return GetMaxCapacity() - ActiveSessions.Count;
        }

    }
}
