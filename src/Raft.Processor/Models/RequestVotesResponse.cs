namespace Raft.Node
{
    public class RequestVotesResponse
    {
        public int CurrentTerm { get; set; }
        public bool Vote { get; set; }
    }
}
