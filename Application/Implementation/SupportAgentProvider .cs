using Application.Interfaces;
using ChatServiceApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Application.Implementation
{
    public class SupportAgentProvider : ISupportAgentProvider
    {
        private readonly Dictionary<ShiftType, List<SupportAgent>> _shiftAssignments;
        private readonly List<SupportAgent> _overflowTeam;
        private readonly List<ShiftSchedule> _shiftTimings;


        // here my note  : You used the constructor in your SupportAgentProvider class to initialize and
        // store predefined static data (team assignments, shift timings, and overflow team) once when the
        // singleton instance is created. Since the service is registered as a singleton, the constructor
        // will be called only once during the application's lifetime, which is exactly what you want for

        public SupportAgentProvider()
        {
            var teamA = new List<SupportAgent>
        {
            new SupportAgent { Name = "Alice", Seniority = AgentSeniority.TeamLead },
            new SupportAgent { Name = "Bob", Seniority = AgentSeniority.MidLevel },
            new SupportAgent { Name = "Cara", Seniority = AgentSeniority.Junior },
            new SupportAgent { Name = "Dane", Seniority = AgentSeniority.Junior }
        };

            var teamB = new List<SupportAgent>
        {
            new SupportAgent { Name = "Kasun", Seniority = AgentSeniority.TeamLead },
            new SupportAgent { Name = "Rangana", Seniority = AgentSeniority.MidLevel },
            new SupportAgent { Name = "Nimal", Seniority = AgentSeniority.MidLevel },
            new SupportAgent { Name = "Anne", Seniority = AgentSeniority.Junior }
        };

            var teamC = new List<SupportAgent>
        {
            new SupportAgent { Name = "Kane", Seniority = AgentSeniority.MidLevel },
            new SupportAgent { Name = "Abdul", Seniority = AgentSeniority.MidLevel }
        };

            _overflowTeam = new List<SupportAgent>
        {
            new SupportAgent { Name = "Overflow1", Seniority = AgentSeniority.Junior, IsOverflow = true },
            new SupportAgent { Name = "Overflow2", Seniority = AgentSeniority.Junior, IsOverflow = true },
            new SupportAgent { Name = "Overflow3", Seniority = AgentSeniority.Junior, IsOverflow = true },
            new SupportAgent { Name = "Overflow4", Seniority = AgentSeniority.Junior, IsOverflow = true },
            new SupportAgent { Name = "Overflow5", Seniority = AgentSeniority.Junior, IsOverflow = true },
            new SupportAgent { Name = "Overflow6", Seniority = AgentSeniority.Junior, IsOverflow = true }
        };

            _shiftAssignments = new Dictionary<ShiftType, List<SupportAgent>>
            {
                [ShiftType.Morning] = teamA,
                [ShiftType.Evening] = teamB,
                [ShiftType.Night] = teamC
            };

            _shiftTimings = new List<ShiftSchedule>
        {
            new(ShiftType.Morning, new(6, 0, 0), new(14, 0, 0)),
            new(ShiftType.Evening, new(14, 0, 0), new(22, 0, 0)),
            new(ShiftType.Night, new(22, 0, 0), new(6, 0, 0))
        };
        }

        public List<SupportAgent> GetCurrentAgents()
        {
            var now = DateTime.Now.TimeOfDay;
            var currentShift = ShiftUtility.GetCurrentShift(_shiftTimings, now);
            var activeAgents = _shiftAssignments[currentShift];
            bool isOfficeHours = now >= TimeSpan.FromHours(8) && now <= TimeSpan.FromHours(18);

            return isOfficeHours ? activeAgents.Concat(_overflowTeam).ToList() : activeAgents;
        }
    }

}
