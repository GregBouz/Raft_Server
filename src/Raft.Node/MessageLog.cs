namespace Raft.Node
{
    public class MessageLog
    {
        private readonly Dictionary<int, LogEntry> _logEntries;

        public MessageLog()
        {
            _logEntries = new Dictionary<int, LogEntry>();
        }

        public void AddLogEntry(int term, int index, string command)
        {
            if (!_logEntries.ContainsKey(index))
                _logEntries.Add(index, new LogEntry() { Command = command, Index = index, Term = term });
        }
    }
}
