namespace ChatServiceApi
{
    public class AgentEfficiency
    {
        public static double Get(AgentSeniority level) => level switch
        {
            AgentSeniority.Junior => 0.4,
            AgentSeniority.MidLevel => 0.6,
            AgentSeniority.Senior => 0.8,
            AgentSeniority.TeamLead => 0.5,
            _ => 0.0
        };
    }
}
