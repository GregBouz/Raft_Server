namespace Raft.Processor
{
    public class Term
    {
        public int Index { get; set; }
        public List<string> VotesReceived { get; set; }
        public bool Voted { get; set; }
    }
}
