using Application.Interfaces;

namespace ChatServiceApi
{
    public class InMemoryChatQueueService : IChatQueueService
    {
        private readonly Queue<ChatSession> _queue = new();
        private readonly List<ChatSession> _allChats = new();
        private readonly object _sync = new();
        private readonly Dictionary<AgentSeniority, int> _rotation = new();
        private readonly ISupportAgentProvider _agentProvider;
        private int? _frozenQueueCount = null; // nullable to check if set
        public InMemoryChatQueueService(ISupportAgentProvider agentProvider)
        {
            _agentProvider = agentProvider;
            Task.Run(CheckPollStatus);
            Task.Run(AssignChats);
        }

        public async Task<string> StartChatAsync(string username)
        {
            lock (_sync)
            {
                var agents = _agentProvider.GetCurrentAgents();

                var mainAgents = agents.Where(a => !a.IsOverflow).ToList();
                var overflowAgents = agents.Where(a => a.IsOverflow).ToList();

                int mainQueueLimit = CalculateQueueLimit(mainAgents);
                int overflowQueueLimit = CalculateQueueLimit(overflowAgents);

                int currentMainCount = _queue.Count(s => !s.IsOverflow);
                int currentOverflowCount = _queue.Count(s => s.IsOverflow);

                if (currentMainCount < mainQueueLimit)
                {
                    var session = new ChatSession { Username = username, State = "OK", IsOverflow = false };
                    _queue.Enqueue(session);
                    _allChats.Add(session);
                    return "OK";
                }

                return "No agents available at the moment.";
            }
        }

        public string Ping(string username)
        {
            lock (_sync)
            {
                var session = _allChats.FirstOrDefault(c => c.Username == username);
                if (session == null || session.IsInactive) return "NOK";

                session.MissedPollCount = 0;
                return "OK";
            }
        }
        private async Task CheckPollStatus()
        {
            while (true)
            {
                lock (_sync)
                {
                    foreach (var session in _allChats)
                    {
                        // Only check unassigned sessions (waiting in queue)
                        if (session.AssignedAgentName == null)
                        {
                            session.MissedPollCount++;

                            if (session.MissedPollCount >= 3)
                            {
                                session.State = "NOK";
                                session.IsInactive = true;
                            }
                        }
                    }
                }

                await Task.Delay(1000); // run every 1 second
            }
        }

        private async Task AssignChats()
        {
            while (true)
            {
                lock (_sync)
                {
                    var agents = _agentProvider.GetCurrentAgents();
                    var mainAgents = agents.Where(a => !a.IsOverflow).ToList();
                    int mainQueueLimit = CalculateQueueLimit(mainAgents);

                    while (_queue.Count > 0)
                    {
                        var session = _queue.Peek();

                        var prioritizedAgents = GetPrioritizedAgents(agents, mainQueueLimit);

                        // Freeze the queue count only once
                        if (_queue.Count == mainQueueLimit)
                            _frozenQueueCount = _queue.Count;

                        if (IsOverflowCondition(prioritizedAgents, agents, mainQueueLimit))
                        {
                            prioritizedAgents = GetOverflowAgents(agents);
                        }

                        if (!TryAssignSessionToAgent(session, prioritizedAgents))
                            break;

                        _queue.Dequeue(); // only after successful assignment
                    }
                }

                await Task.Delay(1000);
            }
        }

        private List<IGrouping<AgentSeniority, SupportAgent>> GetPrioritizedAgents(List<SupportAgent> agents, int mainQueueLimit)
        {
            return agents
                .Where(a => a.GetAvailableSlots() > 0 && !a.IsOverflow)
                .GroupBy(a => a.Seniority)
                .OrderBy(g => g.Key)
                .ToList();
        }

        private bool IsOverflowCondition(List<IGrouping<AgentSeniority, SupportAgent>> prioritizedAgents, List<SupportAgent> agents, int mainQueueLimit)
        {
            return prioritizedAgents.Count == 0 &&
                   _frozenQueueCount == mainQueueLimit &&
                   agents.Any(x => x.IsOverflow);
        }

        private List<IGrouping<AgentSeniority, SupportAgent>> GetOverflowAgents(List<SupportAgent> agents)
        {
            return agents
                .Where(a => a.GetAvailableSlots() > 0 && a.IsOverflow)
                .GroupBy(a => a.Seniority)
                .OrderBy(g => g.Key)
                .ToList();
        }

        private bool TryAssignSessionToAgent(ChatSession session, List<IGrouping<AgentSeniority, SupportAgent>> prioritizedAgents)
        {
            foreach (var group in prioritizedAgents)
            {
                var members = group.ToList();

                if (!_rotation.ContainsKey(group.Key))
                    _rotation[group.Key] = 0;

                for (int i = 0; i < members.Count; i++)
                {
                    int idx = (_rotation[group.Key] + i) % members.Count;
                    var agent = members[idx];

                    if (agent.GetAvailableSlots() > 0)
                    {
                        agent.ActiveSessions.Add(session.Username);
                        session.AssignedAgentName = agent.Name;
                        _rotation[group.Key] = (idx + 1) % members.Count;
                        return true;
                    }
                }
            }

            return false;
        }

        public int CalculateQueueLimit(List<SupportAgent> agents)
        {
            int totalCapacity = agents.Sum(a => a.GetMaxCapacity());
            return (int)(totalCapacity * 1.5);
        }

    }
}
