﻿namespace Raft.Node
{
    public class AppendMessageConfirmationRequest
    {
        public int Term { get; set; }

        public string Message { get; set; }
    }
}
