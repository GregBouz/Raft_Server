using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public enum Role
    {
        Follower = 0,
        Candidate = 1,
        Leader = 2
    }
}
