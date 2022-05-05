using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public class Constituent
    {
        public string Address { get; set; }

        public LogEntry LastEntry { get; set; }
    }
}
