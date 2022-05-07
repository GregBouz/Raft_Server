using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public class TestingPresets
    {
        public int CurrentTerm { get; set; }

        public MessageLog MessageLog { get; set; }
    }
}
