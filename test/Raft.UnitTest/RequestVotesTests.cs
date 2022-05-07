using FluentAssertions;
using Moq;
using Raft.Node;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Raft.UnitTest
{
    public class RequestVotesTests
    {
        [Fact]
        public async void SendRequestVotesTest()
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var _actualTerm = 0;
            var _actualLastLogTerm = 0;
            var _actualLastLogIndex = 0;

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new Dictionary<string, Constituent>()
                {
                    { "test-address-1", new Constituent()
                        {
                            Address = "test-address-1"
                        }
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration);
            node.OnVoteRequest += (sender, serverAddress, term, lastLogTerm, lastLogIndex) => {
                _actualTerm = term;
                _actualLastLogTerm = lastLogTerm;
                _actualLastLogIndex = lastLogIndex;
                _autoResetEvent.Set(); 
            };

            // When the node is started
            node.Start();
            Thread.Sleep(300);

            // Then the election timeout should raise an OnVoteRequest event
            _autoResetEvent.WaitOne().Should().BeTrue();
            _actualTerm.Should().Be(1);
            _actualLastLogTerm.Should().Be(0);
            _actualLastLogTerm.Should().Be(0);
            // Then the current term should have been increased
            node.CurrentTerm.Should().Be(1);
            node.VotesReceived.Should().Be(1);
        }

        [Fact]
        public async void SendRequestVotesMultipleServersTest()
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var _actualTerm = 0;
            var _actualLastLogTerm = 0;
            var _actualLastLogIndex = 0;
            var _serversCalled = 0;

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new Dictionary<string, Constituent>()
                {
                    { "test-address-1", new Constituent()
                        {
                            Address = "test-address-1"
                        }
                    },
                    { "test-address-2", new Constituent()
                        {
                            Address = "test-address-2"
                        }
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration);
            node.OnVoteRequest += (sender, serverAddress, term, lastLogTerm, lastLogIndex) => {
                _serversCalled++;
                _actualTerm = term;
                _actualLastLogTerm = lastLogTerm;
                _actualLastLogIndex = lastLogIndex;
                _autoResetEvent.Set();
            };

            // When the node is started
            node.Start();
            Thread.Sleep(300);

            // Then the election timeout should raise an OnVoteRequest event
            _autoResetEvent.WaitOne().Should().BeTrue();
            _serversCalled.Should().Be(2);
            _actualTerm.Should().Be(1);
            _actualLastLogTerm.Should().Be(0);
            _actualLastLogTerm.Should().Be(0);
            // Then the current term should have been increased
            node.CurrentTerm.Should().Be(1);
            node.VotesReceived.Should().Be(1);
        }

        [Fact]
        public async void SendRequestVotesMultipleServersWithResponsesTest()
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var _actualTerm = 0;
            var _actualLastLogTerm = 0;
            var _actualLastLogIndex = 0;
            var _serversCalled = 0;

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new Dictionary<string, Constituent>()
                {
                    { "test-address-1", new Constituent()
                        {
                            Address = "test-address-1"
                        }
                    },
                    { "test-address-2", new Constituent()
                        {
                            Address = "test-address-2"
                        }
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration);
            node.OnVoteRequest += (sender, serverAddress, term, lastLogTerm, lastLogIndex) => {
                _serversCalled++;
                _actualTerm = term;
                _actualLastLogTerm = lastLogTerm;
                _actualLastLogIndex = lastLogIndex;
                _autoResetEvent.Set();
            };

            node.OnAppendEntries += (sender, receiverAddress, term, index, message, lastLogTerm, lastLogIndex) =>
            {
                _autoResetEvent.Set();
            };

            // When the node is started
            node.Start();
            Thread.Sleep(300);
            node.RequestVotesResponseReceived("test-address-1", true, -1);

            // Then the election timeout should raise an OnVoteRequest event
            _autoResetEvent.WaitOne().Should().BeTrue();
            _autoResetEvent.WaitOne().Should().BeTrue();
            _autoResetEvent.WaitOne().Should().BeTrue();
            _serversCalled.Should().Be(2);
            _actualTerm.Should().Be(1);
            _actualLastLogTerm.Should().Be(0);
            _actualLastLogTerm.Should().Be(0);

            // Then the current term should have been increased
            node.CurrentTerm.Should().Be(1);
            node.VotesReceived.Should().Be(2);
            node.Role.Should().Be(Role.Leader);
        }

        [Fact]
        public async void SendRequestVotesMultipleServersWithFalseResponseTest()
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var _actualTerm = 0;
            var _actualLastLogTerm = 0;
            var _actualLastLogIndex = 0;
            var _serversCalled = 0;

            // Given a node configuration of 1 constituent
            var nodeConfiguration = new NodeConfiguration()
            {
                Constituents = new Dictionary<string, Constituent>()
                {
                    { "test-address-1", new Constituent()
                        {
                            Address = "test-address-1"
                        }
                    },
                    { "test-address-2", new Constituent()
                        {
                            Address = "test-address-2"
                        }
                    }
                }
            };

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration);
            node.OnVoteRequest += (sender, serverAddress, term, lastLogTerm, lastLogIndex) => {
                _serversCalled++;
                _actualTerm = term;
                _actualLastLogTerm = lastLogTerm;
                _actualLastLogIndex = lastLogIndex;
                _autoResetEvent.Set();
            };

            // When the node is started
            node.Start();
            Thread.Sleep(300);
            node.RequestVotesResponseReceived("test-address-1", false, 2);

            // Then the election timeout should raise an OnVoteRequest event
            _autoResetEvent.WaitOne().Should().BeTrue();
            _serversCalled.Should().Be(2);
            _actualTerm.Should().Be(1);
            _actualLastLogTerm.Should().Be(0);
            _actualLastLogTerm.Should().Be(0);
            // Then the current term should have been increased
            node.CurrentTerm.Should().Be(2);
            node.VotesReceived.Should().Be(1);
            node.Role.Should().Be(Role.Follower);
        }
    }
}