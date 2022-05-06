namespace Raft.Node
{
    public class AppendEntiresRequest
    {
        public int Term { get; set; }

        public string Message { get; set; }
    }
}
