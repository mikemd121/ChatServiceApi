namespace ChatServiceApi
{
   public interface IChatQueueService
    {
        Task<string> StartChatAsync(string username);
        string Ping(string username);
    }
}
