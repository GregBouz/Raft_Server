namespace Raft.Node
{
    public class AppendMessageRequest
    {
        public int Term { get; set; }

        public string Message { get; set; }
    }
}
