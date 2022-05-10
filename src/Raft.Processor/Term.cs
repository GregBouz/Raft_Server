using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public class Term
    {
        public int Index { get; set; }
        public List<string> VotesReceived { get; set; }
        public bool Voted { get; set; }
    }
}
