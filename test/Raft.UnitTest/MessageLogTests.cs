using FluentAssertions;
using Raft.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Raft.UnitTest
{
    public class MessageLogTests
    {
        /// <summary>
        /// Tests that finding a log entry returns success
        /// </summary>
        [Fact]
        public async void DoesLastLogEntryMatchSuccessTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 0, "test-message-1");
            messageLog.AddLogEntry(2, 1, "test-message-2");
            messageLog.AddLogEntry(2, 2, "test-message-3");

            // When DoesLastLogEntryMatch is called
            var result = messageLog.DoesLastLogEntryMatch(2, 2);

            // Then is the result expected
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that finding a log entry that doesn't exist returns fail
        /// </summary>
        [Fact]
        public async void DoesLastLogEntryMatchFailInvalidIndexTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 0, "test-message-1");
            messageLog.AddLogEntry(2, 1, "test-message-2");
            messageLog.AddLogEntry(2, 2, "test-message-3");

            // When DoesLastLogEntryMatch is called
            var result = messageLog.DoesLastLogEntryMatch(3, 2);

            // Then is the result expected
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that finding a log entry that exists at the index but doesn't match term returns fail
        /// </summary>
        [Fact]
        public async void DoesLastLogEntryMatchFailInvalidTermTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();
            messageLog.AddLogEntry(1, 0, "test-message-1");
            messageLog.AddLogEntry(2, 1, "test-message-2");
            messageLog.AddLogEntry(2, 2, "test-message-3");

            // When DoesLastLogEntryMatch is called
            var result = messageLog.DoesLastLogEntryMatch(2, 3);

            // Then is the result expected
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that adding a log entry succeeds
        /// </summary>
        [Fact]
        public async void AddLogEntrySuccessTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();

            // When adding log entries
            messageLog.AddLogEntry(1, 1, "test-message-1");
            messageLog.AddLogEntry(2, 2, "test-message-2");
            messageLog.AddLogEntry(2, 3, "test-message-3");
            var result = messageLog.AddLogEntry(3, 4, "test-message-4");

            // Then is the result expected
            result.Should().NotBeNull();
            result.Term.Should().Be(3);
            result.Command.Should().Be("test-message-4");
        }

        /// <summary>
        /// Tests that adding a log entry without the correct next index fails
        /// </summary>
        [Fact]
        public async void AddLogEntryFailAlreadyExistsTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();

            // When adding log entries
            messageLog.AddLogEntry(1, 1, "test-message-1");
            messageLog.AddLogEntry(2, 2, "test-message-2");
            messageLog.AddLogEntry(2, 3, "test-message-3");
            var result = messageLog.AddLogEntry(2, 3, "test-message-4");

            // Then is the result expected
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that adding a log entry without the correct next index fails
        /// </summary>
        [Fact]
        public async void AddLogEntryFailSkipsNextIndexTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();

            // When adding log entries
            messageLog.AddLogEntry(1, 1, "test-message-1");
            messageLog.AddLogEntry(2, 2, "test-message-2");
            messageLog.AddLogEntry(2, 3, "test-message-3");
            var result = messageLog.AddLogEntry(2, 5, "test-message-4");

            // Then is the result expected
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests clearing the log including and after a specified index
        /// </summary>
        [Fact]
        public async void ClearLogEntryAndAllAfterTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();

            // When adding log entries
            messageLog.AddLogEntry(1, 1, "test-message-1");
            messageLog.AddLogEntry(2, 2, "test-message-2");
            messageLog.AddLogEntry(2, 3, "test-message-3");
            var result = messageLog.ClearLogEntryAndAllAfter(2);

            // Then is the result expected
            result.Count.Should().Be(1);
        }

        /// <summary>
        /// Tests clearing the log including and after a specified index (last index)
        /// </summary>
        [Fact]
        public async void ClearLogEntryAndAllAfterLastIndexTest()
        {
            // Given a message log with three entries
            var messageLog = new MessageLog();

            // When adding log entries
            messageLog.AddLogEntry(1, 1, "test-message-1");
            messageLog.AddLogEntry(2, 2, "test-message-2");
            messageLog.AddLogEntry(2, 3, "test-message-3");
            var result = messageLog.ClearLogEntryAndAllAfter(3);

            // Then is the result expected
            result.Count.Should().Be(2);
            result[0].Command.Should().Be("test-message-1");
            result[1].Command.Should().Be("test-message-2");
        }
    }
}
