using Application.Implementation;
using Application.Interfaces;
using ChatServiceApi;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ChatQueueServiceTests
    {

        // Test Case : A team of 2 people: 1 snr(cap 8), 1 jnr (cap 4).
        //Input: 5 chats arrive.
        //Result: 4 of which would be assigned to the jnr and 1 to the senior
        [Fact]
        public async Task Test_AssignChats_JuniorFilledBeforeSenior_WhenCapacityAllows()
        {

            var agents = new List<SupportAgent>
                {
                    new SupportAgent { Name = "Agent1", Seniority = AgentSeniority.Senior },
                    new SupportAgent { Name = "Agent2", Seniority = AgentSeniority.Junior }
                };

            var mockProvider = new Mock<ISupportAgentProvider>();
            mockProvider.Setup(p => p.GetCurrentAgents()).Returns(agents);

            var service = new InMemoryChatQueueService(mockProvider.Object);

            for (int i = 0; i < 5; i++)
            {
                var session = new ChatSession { Username = $"user{i}", IsOverflow = false };
                await service.StartChatAsync(session.Username);

            }
            await Task.Delay(2000);
            var juniorAllocatedChatCount = agents
                .Where(x => x.Seniority == AgentSeniority.Junior)
                .SelectMany(x => x.ActiveSessions)
                .Count();

            var seniorAllocatedChatCount = agents
                 .Where(x => x.Seniority == AgentSeniority.Senior)
                 .SelectMany(x => x.ActiveSessions)
                 .Count();

            Assert.Equal(4, juniorAllocatedChatCount);
            Assert.Equal(1, seniorAllocatedChatCount);

        }


        //Test Case : Team of 2jnr 1mid 6 chats arrive.
        //Input: 6 chats arrive.
        //Result: 3 each to the jnr, non to the mid
        [Fact]
        public async Task Test_ChatAssignment_JuniorPreferredOverMid()
        {

            var agents = new List<SupportAgent>
                {
                    new SupportAgent { Name = "Agent1", Seniority = AgentSeniority.MidLevel },
                    new SupportAgent { Name = "Agent2", Seniority = AgentSeniority.Junior },
                    new SupportAgent { Name = "Agent3", Seniority = AgentSeniority.Junior }
                };

            var mockProvider = new Mock<ISupportAgentProvider>();
            mockProvider.Setup(p => p.GetCurrentAgents()).Returns(agents);

            var service = new InMemoryChatQueueService(mockProvider.Object);

            for (int i = 0; i < 6; i++)
            {
                var session = new ChatSession { Username = $"user{i}", IsOverflow = false };
                await service.StartChatAsync(session.Username);

            }
            await Task.Delay(2000);
            var juniorAllocatedChatCount = agents
                .Where(x => x.Seniority == AgentSeniority.Junior)
                .SelectMany(x => x.ActiveSessions)
                .Count();

            var midLevelAllocatedChatCount = agents
                 .Where(x => x.Seniority == AgentSeniority.MidLevel)
                 .SelectMany(x => x.ActiveSessions)
                 .Count();

            Assert.Equal(6, juniorAllocatedChatCount);
            Assert.Equal(0, midLevelAllocatedChatCount);

        }

        [Fact]
        public void CalculateQueueLimit_ShouldReturnExpectedLimit()
        {
            var agents = new List<SupportAgent>
            {
                new SupportAgent { Seniority = AgentSeniority.Junior },
                new SupportAgent { Seniority = AgentSeniority.Senior },
                new SupportAgent { Seniority = AgentSeniority.MidLevel }
             };

            var supportAgentProvider = new SupportAgentProvider();
            var service = new InMemoryChatQueueService(supportAgentProvider); // Replace with your real class containing the method
            var result = service.CalculateQueueLimit(agents);
            int expectedTotal = 4 + 8 + 6;         // 18
            int expectedLimit = (int)(expectedTotal * 1.5);  // 27
            Assert.Equal(expectedLimit, result);
        }

        [Fact]
        public async Task IntiateChat_ShouldReturnOk()
        {
            var mock = new Mock<ISupportAgentProvider>();
            var service = mock.Setup(x => x.GetCurrentAgents()).Returns(GetAgentList());
            var classCall = new InMemoryChatQueueService(mock.Object);
            var result = await classCall.StartChatAsync("sss");

            Assert.Equal("OK", result);

        }

        [Fact]
        public async Task StartChat_ShouldReturnNoAgents_WhenMainAndOverflowLimitsReached()
        {

            var agents = new List<SupportAgent>
                {
                    new SupportAgent { Name = "Agent1", Seniority = AgentSeniority.Junior },
                    new SupportAgent { Name = "Agent2", Seniority = AgentSeniority.Junior }
                };

            // here total capacity is 8 and max queue length is 8*1.5 =12.
            // so 12 chats can handle by the agents concurrently. and also after agents are filled, can fill the
            // queueup to 12. so totally 24 queue msgs should be sent.so when it exceed 24 +1 msgs it should fire queue is full.
            var mockProvider = new Mock<ISupportAgentProvider>();
            mockProvider.Setup(p => p.GetCurrentAgents()).Returns(agents);

            var service = new InMemoryChatQueueService(mockProvider.Object);

            for (int i = 0; i < 25; i++)
            {
                var session = new ChatSession { Username = $"user{i}", IsOverflow = false };
                await service.StartChatAsync(session.Username);
            }

            string result = await service.StartChatAsync("newUser");
            Assert.Equal("No agents available at the moment.", result);
        }

        private static List<SupportAgent> GetAgentList()
        {
            return new List<SupportAgent>
        {
            new SupportAgent { Name = "Alice", Seniority = AgentSeniority.TeamLead },
            new SupportAgent { Name = "Bob", Seniority = AgentSeniority.MidLevel },
            new SupportAgent { Name = "Cara", Seniority = AgentSeniority.Junior },
            new SupportAgent { Name = "Dane", Seniority = AgentSeniority.Junior }
        };

        }
    }
}
