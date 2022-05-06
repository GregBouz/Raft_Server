namespace Raft.Node
{
    public class VoteRequest
    {
        public int Term { get; set; }
        public int Index { get; set; }
    }
}
