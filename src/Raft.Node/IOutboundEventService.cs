using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public interface IOutboundEventService
    {
        public void SendAppendEntries(string address, AppendEntiresRequest request);

        public void SendVoteRequest(string address);
    }
}
