﻿namespace Raft.Node
{
    public class RequestVotesRequest
    {
        public int Term { get; set; }
        public int Index { get; set; }
        public int LastLogTerm { get; set; }
        public int LastLogIndex { get; set; }
    }
}
