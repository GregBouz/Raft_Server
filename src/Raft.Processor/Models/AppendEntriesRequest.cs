namespace Raft.Processor
{
    public class AppendEntriesRequest
    {
        public int LeaderCommit { get; set; }
        public int LeaderIndex { get; set; }
        public int prevLogIndex { get; set; }
        public int prevLogTerm { get; set; }
        public int Term { get; set; }
        public LogEntry[] LogEntries { get; set; }
    }
}
