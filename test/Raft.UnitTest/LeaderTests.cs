using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.UnitTest
{
    /// <summary>
    /// A leader should:
    /// 1) Send heartbeat upon receiving majority votes
    /// 2) Send heartbeat on heartbeat timeout
    /// 3) On receiving client command append entry to message log
    /// 3a) Send appendEntries to each server
    /// 3b) If a servers nextIndex is behind then send all missing log entries
    /// 3c) On success from follower update their next log index
    /// 3d) If follower response fail decrese their next log index and retry
    /// 
    /// </summary>
    internal class LeaderTests
    {
    }
}
