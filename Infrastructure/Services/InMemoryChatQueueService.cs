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
        public InMemoryChatQueueService(ISupportAgentProvider agentProvider)
        {
            _agentProvider = agentProvider;
            Task.Run(CheckPollStatus);
            Task.Run(AssignChats);
        }
        private bool IsOfficeHours()
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= TimeSpan.FromHours(8) && now <= TimeSpan.FromHours(18);
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

                    if (IsOfficeHours() && currentOverflowCount < overflowQueueLimit)
                    {
                        var session = new ChatSession { Username = username, State = "OK", IsOverflow = true };
                        _queue.Enqueue(session);
                        _allChats.Add(session);
                        return "OK (Overflow)";
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

                    while (_queue.Count > 0)
                    {
                        var session = _queue.Peek();
                        //if (session.IsInactive)
                        //{
                        //    _queue.Dequeue();
                        //    continue;
                        //}

                        List<IGrouping<AgentSeniority, SupportAgent>> prioritized;
                        prioritized = agents
                         .Where(a => a.GetAvailableSlots() > 0 && !a.IsOverflow)
                         .GroupBy(a => a.Seniority)
                         .OrderBy(g => g.Key).ToList();

                        if (prioritized.Count() == 0 && agents.Where(x => x.IsOverflow).Any())
                        {
                            prioritized = agents
                               .Where(a => a.GetAvailableSlots() > 0 && a.IsOverflow)
                               .GroupBy(a => a.Seniority)
                               .OrderBy(g => g.Key).ToList();

                        }


                        bool isAssigned = false;

                        foreach (var group in prioritized)
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
                                    _queue.Dequeue();
                                    isAssigned = true;
                                    break;
                                }
                            }
                            if (isAssigned) break;
                        }

                        if (!isAssigned) break;
                    }
                }
                await Task.Delay(1000);
            }
        }

        public int CalculateQueueLimit(List<SupportAgent> agents)
        {
            int totalCapacity = agents.Sum(a => a.GetMaxCapacity());
            return (int)(totalCapacity * 1.5);
        }

    }
}
