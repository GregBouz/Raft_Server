using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node.Models
{
    public class RequestVotesResponse
    {
        public int CurrentTerm { get; set; }
        public bool Vote { get; set; }
    }
}
