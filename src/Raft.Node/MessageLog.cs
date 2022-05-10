namespace Raft.Node
{
    /// <summary>
    /// An implementation of a sequential log storage that starts with the first entry having an index of 1
    /// and increasing by 1 with each entry.
    /// </summary>
    public class MessageLog
    {
        private readonly List<LogEntry> _logEntries;

        public int CommitIndex { get; set; }

        public MessageLog()
        {
            _logEntries = new List<LogEntry>();
            CommitIndex = 0;
        }

        public LogEntry? LastLogEntry()
        {
            return _logEntries.Count == 0 ? null : _logEntries.Last();
        }

        public int LastLogIndex()
        {
            return _logEntries.Count;
        }

        public int LastLogTerm()
        {
            return _logEntries.Count == 0 ? 0 : _logEntries.Last().Term;
        }

        public LogEntry AddLogEntry(int term, int index, string command)
        {
            var newLogEntry = new LogEntry() { Command = command, Index = index, Term = term };
            // Ensure the index specified is the next index for the log
            if (index == _logEntries.Count + 1)
            {
                _logEntries.Add(newLogEntry);
                CommitIndex++;
                return newLogEntry;
            }
            return null;
        }

        public List<LogEntry> ClearLogEntryAndAllAfter(int index)
        {
            _logEntries.RemoveRange(index - 1, _logEntries.Count - (index - 1));
            return _logEntries;
        }

        public bool DoesLastLogEntryMatch(int index, int term)
        {
            var lastEntry = LastLogEntry();
            if (lastEntry == null)
                return false;
            return lastEntry.Index == index && lastEntry.Term == term;
        }
    }
}
