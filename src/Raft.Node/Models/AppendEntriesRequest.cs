namespace Raft.Node
{
    public class AppendEntriesRequest
    {
        public int Index { get; set; }
        public int Term { get; set; }
        public string Message { get; set; }
    }
}
