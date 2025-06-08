using ChatServiceApi;

namespace Application.Interfaces
{
   public interface ISupportAgentProvider
    {
        List<SupportAgent> GetCurrentAgents();
    }
}
