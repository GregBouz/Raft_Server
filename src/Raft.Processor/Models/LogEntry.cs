namespace Raft.Processor
{
    /// <summary>
    /// Represents an entry in the replicated log.
    /// </summary>
    public class LogEntry
    {
        public string Command { get; set; }
        public int Index { get; set; }
        public int Term { get; set; }
    }
}
