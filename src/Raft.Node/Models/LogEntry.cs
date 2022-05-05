namespace Raft.Node
{
    public class LogEntry
    {
        public int Term { get; set; }
        public string Message { get; set; }
    }
}
