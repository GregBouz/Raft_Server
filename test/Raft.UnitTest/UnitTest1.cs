using Moq;
using Raft.Node;
using System.Threading;
using Xunit;

namespace Raft.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public async void ElectionTimeoutTest()
        {
            // Given a mocked outbound event service
            var eventServiceMock = new Mock<IOutboundEventService>();

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new System.Collections.Generic.List<Constituent>()
                {
                    new Constituent()
                    {
                        Address = "test-address-1"
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor(1, nodeConfiguration, eventServiceMock.Object);
            
            // When the node is started
            node.Start();
            Thread.Sleep(300);

            // Then the election timeout should trigger and attempt to send an election message
            eventServiceMock.Verify(e => e.SendElection("test-address-1"), Times.Once());
        }

        [Fact]
        public async void HeartbeatTimeoutTest()
        {
            // Given a mocked outbound event service
            var eventServiceMock = new Mock<IOutboundEventService>();

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new System.Collections.Generic.List<Constituent>()
                {
                    new Constituent()
                    {
                        Address = "test-address-1"
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor(1, nodeConfiguration, eventServiceMock.Object);

            // When the node is started
            node.Start();
            Thread.Sleep(300);

            // Then the election timeout should trigger and attempt to send an election message
            eventServiceMock.Verify(e => e.SendElection("test-address-1"), Times.Once());
        }
    }
}