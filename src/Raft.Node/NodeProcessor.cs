using System.Net;
using System.Net.Sockets;

namespace Raft.Node
{
    /// <summary>
    /// TODO:
    /// Add functions for handling recieved events (heartbeat, voterequest, appendMessage)
    /// </summary>
    public class NodeProcessor
    {
        private IOutboundEventService _outboundEventService;
        private NodeConfiguration _nodeConfiguration;

        private Timer _electionTimer;
        private Timer _heartbeatTimer;
        private int _electionTimeoutInMilliSeconds = new Random().Next(150, 300);
        private int _heartbeatTimeoutInMilliSeconds = new Random().Next(150, 300);
        private DateTimeOffset _lastElection;

        private TcpListener _requestListener;

        private MessageLog _messageLog;
        private int _currentTerm = 0;

        public Role Role { get; private set; }
        public ElectionRequest? _electionRequest;

        /// <summary>
        /// Initialize the node
        /// </summary>
        public NodeProcessor(int serverNode, NodeConfiguration nodeConfiguration, IOutboundEventService outboundEventService)
        {
            //IPAddress ipaddress = Dns.GetHostEntry("localhost").AddressList[0];
            //_requestListener = new TcpListener(ipaddress, 8000 + serverNode);
            //_requestListener.Start();
            //_lastElection = DateTimeOffset.UtcNow;

            _messageLog = new MessageLog();
            _outboundEventService = outboundEventService;
            _nodeConfiguration = nodeConfiguration;

            //ProcessLoop();
        }

        public void Start()
        {
            Role = Role.Follower;
            _electionTimer = new Timer(HandleElectionTimeout, null, _electionTimeoutInMilliSeconds, Timeout.Infinite);
        }

        public void AppendMessageRequest(AppendMessageRequest request)
        {

        }

        public void AppendMessageConfirm(AppendMessageConfirmationRequest request)
        {

        }

        public void HeartBeatRecieved()
        {
            _electionTimer.Change(_electionTimeoutInMilliSeconds, Timeout.Infinite);
        }

        public void VoteRecieved(object? state)
        {

        }

        private void HandleElectionTimeout(object? state)
        {
            Role = Role.Candidate;
            foreach (Constituent constituent in _nodeConfiguration.Constituents.Values)
            {
                _outboundEventService.SendElection(constituent.Address);
            }
        }

        /// <summary>
        /// The main process loop to continually check for the following conditions:
        /// 1) Has the election timeout elapsed?
        /// 2) If so call for an election
        /// 3) Otherwise handle any requests from the current leader
        /// </summary>
        //private void ProcessLoop()
        //{
        //    while (true)
        //    {
        //        HandleRequests();
        //        if (_role == Role.Leader)
        //        {

        //        }
        //        // Check if election timeout has elapsed as either Candidate or Follower
        //        else if (DateTimeOffset.UtcNow > _lastElection.AddMilliseconds(_electionTimeoutInMilliSeconds))
        //        {
        //            _role = Role.Candidate;
        //            _lastElection = DateTimeOffset.UtcNow;
        //            CallForElection();
        //        }
        //    }
        //}

        /// <summary>
        /// Send out a request for all constituents to vote
        /// </summary>
        private void CallForElection()
        {
            // Request a vote from each constituent
            //foreach (Constituent constituent in _constituents)
            //{
                //constituent.Address;
            //}

            // If a majority response then become the leader

            // If the election timeout elapses then send out another vote
        }

        private void HandleRequests()
        {
            _requestListener.AcceptSocket();
            if (_electionRequest != null)
            {

            }
            _electionRequest = null;
        }
    }
}