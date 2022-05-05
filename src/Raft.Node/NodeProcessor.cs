namespace Raft.Node
{
    public class NodeProcessor
    {
        private Role _role = 0;
        private int _electionTimeoutInMilliSeconds = new Random().Next(300, 700);
        private DateTimeOffset _lastElection;

        /// <summary>
        /// Initialize the node
        /// </summary>
        public NodeProcessor()
        {
            _lastElection = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// The main process loop to continually check for the following conditions:
        /// 1) Has the election timeout elapsed?
        /// 2) If so call for an election
        /// 3) Otherwise handle any requests from the current leader
        /// </summary>
        private void ProcessLoop()
        {
            while (true)
            {
                if (DateTimeOffset.UtcNow > _lastElection.AddMilliseconds(_electionTimeoutInMilliSeconds))
                {

                }
            }
        }

        /// <summary>
        /// Send out a request for all constituents to vote
        /// </summary>
        private void CallForElection()
        {

        }
    }
}