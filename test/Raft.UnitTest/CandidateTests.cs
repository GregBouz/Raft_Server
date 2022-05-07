namespace Raft.UnitTest
{
    /// <summary>
    /// A candidate should:
    /// 1) Start an election on becoming a candidate
    /// 1a) Increment the current term
    /// 1b) Vote for self
    /// 1c) Reset the election timer
    /// 1d) Send a requestVote call to all other nodes
    /// 2) Transfer to leader when majority votes are received
    /// 3) Transfer to follower if AppendEntries received from new leader
    /// 4) Start new election if election timeout elapses
    /// </summary>
    internal class CandidateTests
    {
    }
}
