using FluentAssertions;
using Raft.Processor;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Raft.UnitTest
{
    /// <summary>
    /// A follower should:
    /// 1) Respond to AppendEntries with success
    /// 1a) Respond to AppendEntries when request last log index is stale
    /// 1b) Response to AppendEntries when request term is stale
    /// 2) Respond to RequestVotes (RespondToRequestVotesTest) 
    /// 2a) Send fail response if term in request is lower than current term (RespondToRequestVotesFailedOlderTermTest)
    /// 2b) Send fail response if last log index in request is lower than servers (RespondToRequestVotesFailedOlderMessageLogTest)
    /// 2c) Send fail response if server has already voted in term (RespondToRequestVotesFailedAlreadyVotedTest)
    /// 3) Convert to Candidate on ElectionTimeout (HeartbeatTimeoutTest)
    /// 4) Set term to leaders term appendEntries is received (UpdateStaleTermToRequestTermTest)
    /// </summary>
    public class FollowerTests
    {
        /// <summary>
        /// Tests that when the heartbeat timeout elapses that the follower becomes a candidate.
        /// </summary>
        [Fact]
        public async void HeartbeatTimeoutTest()
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
                _actualTerm = term;
                _actualLastLogTerm = lastLogTerm;
                _actualLastLogIndex = lastLogIndex;
                _autoResetEvent.Set();
            };

            // When the node is started and the heartbeat timeout elapses
            node.Start();
            // Then the election timeout should raise an OnVoteRequest event
            _autoResetEvent.WaitOne();
            node.Stop();
            // Then the server should vote true
            node.CurrentTerm.Should().Be(1);
            _actualTerm.Should().Be(1);
            _actualLastLogTerm.Should().Be(0);
            _actualLastLogIndex.Should().Be(0);
        }

        /// <summary>
        /// Tests that a success response is sent if a candidate requests a vote and has a log at least as complete as the servers
        /// </summary>
        [Fact]
        public async void RespondToRequestVotesTest()
        {
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

            // When the node is started and a vote request is made
            node.Start();
            var response = node.RequestVoteReceived("test-address-1", new RequestVotesRequest() { Term = 1, LastLogIndex = 0, LastLogTerm = 0 });
            node.Stop();

            // Then the server should vote true
            response.Vote.Should().BeTrue();
            response.CurrentTerm.Should().Be(1);
        }

        /// <summary>
        /// Tests that a fail response is sent if a candidate requests a vote but their current term is less than the
        /// servers term.
        /// </summary>
        [Fact]
        public async void RespondToRequestVotesFailedOlderTermTest()
        {
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
            var node = new NodeProcessor("test-address-0", nodeConfiguration, new TestingPresets() { CurrentTerm = 3 });

            // When the node is started and a vote request is made
            node.Start();
            var response = node.RequestVoteReceived("test-address-1", new RequestVotesRequest() { Term = 1, LastLogIndex = 0, LastLogTerm = 0 });
            node.Stop();

            // Then the server should vote true
            response.Vote.Should().BeFalse();
            response.CurrentTerm.Should().Be(3);
        }

        /// <summary>
        /// Tests that a fail response is sent if a candidate requests a vote but log is out-of-date compared to the servers.
        /// </summary>
        [Fact]
        public async void RespondToRequestVotesFailedOlderMessageLogTest()
        {
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

            // Given a preset message log
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 1, "1-1-test");
            messageLog.AddLogEntry(1, 2, "1-2-test");
            messageLog.AddLogEntry(2, 3, "2-3-test");
            messageLog.AddLogEntry(3, 4, "3-4-test");

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration, new TestingPresets() { CurrentTerm = 3, MessageLog = messageLog });

            // When the node is started and a vote request is made
            node.Start();
            var response = node.RequestVoteReceived("test-address-1", new RequestVotesRequest() { Term = 3, LastLogIndex = 3, LastLogTerm = 2 });
            node.Stop();

            // Then the server should vote true
            response.Vote.Should().BeFalse();
            response.CurrentTerm.Should().Be(3);
        }

        /// <summary>
        /// Tests that a fail response is sent if a candidate requests a vote to a node that has already voted
        /// </summary>
        [Fact]
        public async void RespondToRequestVotesFailedAlreadyVotedTest()
        {
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

            // When the node is started and a successful vote request is made followed by another vote request
            node.Start();
            var response = node.RequestVoteReceived("test-address-1", new RequestVotesRequest() { Term = 1, LastLogIndex = 0, LastLogTerm = 0 });
            var secondResponse = node.RequestVoteReceived("test-address-2", new RequestVotesRequest() { Term = 1, LastLogIndex = 0, LastLogTerm = 0 });
            node.Stop();

            // Then the server should vote true
            response.Vote.Should().BeTrue();
            secondResponse.Vote.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a the server updates its stale currentTerm when a vote request contains a newer term
        /// </summary>
        [Fact]
        public async void UpdateStaleTermToRequestTermTest()
        {
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

            // When the node is started and a successful vote request is made followed by another vote request
            node.Start();
            var response = node.RequestVoteReceived("test-address-1", new RequestVotesRequest() { Term = 3, LastLogIndex = 0, LastLogTerm = 0 });
            node.Stop();

            // Then the server should vote true
            node.CurrentTerm.Should().Be(3);
        }

        /// <summary>
        /// Tests that a success response is sent if an appendEntries request is made and the server is up-to-date
        /// </summary>
        [Fact]
        public async void RespondToAppendEntriesSuccessTest()
        {
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

            // Given a preset message log
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 1, "1-1-test");
            messageLog.AddLogEntry(1, 2, "1-2-test");
            messageLog.AddLogEntry(2, 3, "2-3-test");
            messageLog.AddLogEntry(3, 4, "3-4-test");

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration, new TestingPresets() { CurrentTerm = 3, MessageLog = messageLog });

            // When the node is started and a vote request is made
            node.Start(false);
            var response = node.AppendEntriesReceived("test-address-1", new AppendEntriesRequest()
            {
                Term = 4,
                LeaderIndex = 1,
                prevLogIndex = 4,
                prevLogTerm = 3,
                LeaderCommit = 5,
                LogEntries =
                new LogEntry[]
                {
                    new LogEntry()
                    {
                        Term = 4,
                        Index = 5,
                        Command = "test-message-5"
                    }
                }
            });

            response.Success.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a failed response is sent if an appendEntries request is made and request term is lower than the servers
        /// </summary>
        [Fact]
        public async void RespondToAppendEntriesFailTermIsLowerTest()
        {
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

            // Given a preset message log
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 1, "1-1-test");
            messageLog.AddLogEntry(1, 2, "1-2-test");
            messageLog.AddLogEntry(2, 3, "2-3-test");
            messageLog.AddLogEntry(3, 4, "3-4-test");

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration, new TestingPresets() { CurrentTerm = 3, MessageLog = messageLog });

            // When the node is started and a vote request is made
            node.Start(false);
            var response = node.AppendEntriesReceived("test-address-1", new AppendEntriesRequest()
            {
                Term = 4,
                LeaderIndex = 1,
                prevLogIndex = 4,
                prevLogTerm = 2,
                LeaderCommit = 4,
                LogEntries =
                new LogEntry[]
                {
                    new LogEntry()
                    {
                        Term = 4,
                        Index = 5,
                        Command = "test-message-5"
                    }
                }
            });

            response.Success.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a failed response is sent if an appendEntries request is made but the existing index has a mismatched term
        /// </summary>
        [Fact]
        public async void RespondToAppendEntriesFailMismatchedTermTest()
        {
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

            // Given a preset message log
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 1, "1-1-test");
            messageLog.AddLogEntry(1, 2, "1-2-test");
            messageLog.AddLogEntry(2, 3, "2-3-test");
            messageLog.AddLogEntry(3, 4, "3-4-test");

            // Given a raft node
            var node = new NodeProcessor("test-address-0", nodeConfiguration, new TestingPresets() { CurrentTerm = 3, MessageLog = messageLog });

            // When the node is started and a vote request is made
            node.Start(false);
            var response = node.AppendEntriesReceived("test-address-1", new AppendEntriesRequest()
            {
                Term = 4,
                LeaderIndex = 1,
                prevLogIndex = 4,
                prevLogTerm = 5,
                LeaderCommit = 5,
                LogEntries =
                new LogEntry[]
                {                    
                    new LogEntry()
                    {
                        Term = 3,
                        Index = 4,
                        Command = "3-4-test"
                    },
                    new LogEntry()
                    {
                        Term = 4,
                        Index = 5,
                        Command = "4-5-test"
                    }
                }
            });

            response.Success.Should().BeFalse();
        }
    }
}
