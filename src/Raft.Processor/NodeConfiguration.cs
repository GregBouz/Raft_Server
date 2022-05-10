using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public class NodeConfiguration
    {
        public Dictionary<string, Constituent> Constituents { get; set; }
    }
}
