namespace Raft.Node
{
    public class MessageLog
    {
        private readonly List<LogEntry> _logEntries;

        public MessageLog()
        {
            _logEntries = new List<LogEntry>();
        }

        public LogEntry? LastLogEntry()
        {
            return _logEntries.Count == 0 ? null : _logEntries.Last();
        }

        public int LastLogIndex()
        {
            return _logEntries.Count == 0 ? 0 : _logEntries.Last().Index;
        }

        public int LastLogTerm()
        {
            return _logEntries.Count == 0 ? 0 : _logEntries.Last().Term;
        }

        public void AddLogEntry(int term, int index, string command)
        {
            if (_logEntries.Count() >= index)
                _logEntries.Add(new LogEntry() { Command = command, Index = index, Term = term });
        }
    }
}
