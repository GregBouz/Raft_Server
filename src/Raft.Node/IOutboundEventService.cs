using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public interface IOutboundEventService
    {
        public void SendHeartbeat(string address);

        public void SendElection(string address);

        public void SendAppendConfirmation(string address, AppendMessageConfirmationRequest request);

        public void SendAppend(string address, AppendMessageRequest request);

        public void SendElectionResponse(string address);
    }
}
