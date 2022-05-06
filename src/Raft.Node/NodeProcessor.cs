using System.Net;
using System.Net.Sockets;

namespace Raft.Node
{
    // Delegates
    public delegate void SendVoteRequest(string serverAddress, int term, int lastLogTerm, int lastLogIndex);
    public delegate void SendAppendEntries();

    /// <summary>
    /// TODO:
    /// Add functions for handling recieved events (heartbeat, voterequest, appendMessage)
    /// </summary>
    public class NodeProcessor
    { 
        private NodeConfiguration _nodeConfiguration;

        private Timer _electionTimer;
        private Timer _heartbeatTimer;
        private int _electionTimeoutInMilliSeconds = new Random().Next(150, 300);
        private int _heartbeatTimeoutInMilliSeconds = new Random().Next(150, 300);
        private DateTimeOffset _lastElection;

        private TcpListener _requestListener;

        private MessageLog _messageLog;

        private bool _hasVoted = false;

        public event SendVoteRequest OnVoteRequest;
        public event SendAppendEntries OnAppendEntries;

        public Role Role { get; private set; }
        public int CurrentTermIndex { get; private set; }
        public int VotesReceived { get; private set; }

        /// <summary>
        /// Initialize the node
        /// </summary>
        public NodeProcessor(int serverNode, NodeConfiguration nodeConfiguration)
        {
            CurrentTermIndex = 0;
            VotesReceived = 0;

            _messageLog = new MessageLog();
            _nodeConfiguration = nodeConfiguration;
        }

        public void Start()
        {
            Role = Role.Follower;
            _electionTimer = new Timer(HandleElectionTimeout, null, _electionTimeoutInMilliSeconds, Timeout.Infinite);
        }

        public AppendEntriesResponse AppendEntriesReceived(string sender, AppendEntriesRequest request)
        {
            // Check for log completeness from the appendEntries request
            if ((request.Term > _messageLog.LastLogTerm()) || 
                ((request.Term >= _messageLog.LastLogTerm()) && (request.Index > _messageLog.LastLogIndex())))
            {
                // Append message to log and respond
                _messageLog.AddLogEntry(request.Term, request.Index, request.Message);
                return new AppendEntriesResponse();
            }
            return null;
        }

        public void AppendEntriesResponseReceived(string sender, AppendEntriesResponse request)
        {
            //_messageLog.AddLogEntry(request.Term, request.Message);
        }

        /// <summary>
        /// If a vote is received update the votes received list until a majority is counted
        /// </summary>
        /// <param name="state"></param>
        public void RequestVotesResponseReceived(string sender)
        {
            VotesReceived++;
            if (VotesReceived > (_nodeConfiguration.Constituents.Count / 2))
            {
                Role = Role.Leader;
                foreach (var constituent in _nodeConfiguration.Constituents)
                {
                    OnAppendEntries.Invoke();
                    //_outboundEventService.SendAppend(constituent.Value.Address, 
                    //    new AppendMessageRequest() { Term = _currentTerm.TermIndex, Message = "" });
                }
            }
        }

        /// <summary>
        /// A vote of no will occur if:
        /// 1) the request has a term less than the current term
        /// 2) a vote has already been made for this term
        /// 3) the requested log is less complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        public void RequestVotes(string sender, RequestVotesRequest request)
        {
            //if ((request.Term < _currentTermIndex) || _hasVoted || )
            //{
            //    _currentTerm.Voted = true;
            //    _outboundEventService.SendElectionResponse(sender);
            //}
        }

        /// <summary>
        /// When the election timeout elapses: 
        /// 1) the server becomes a candidate
        /// 2) the current term is incremented
        /// 3) the server votes for itself
        /// 4) voteRequests are made to all other servers
        /// </summary>
        /// <param name="state"></param>
        private void HandleElectionTimeout(object? state)
        {
            Role = Role.Candidate;
            CurrentTermIndex++;
            VotesReceived = 1;
            foreach (Constituent constituent in _nodeConfiguration.Constituents.Values)
            {
                OnVoteRequest.Invoke(constituent.Address, CurrentTermIndex, _messageLog.LastLogTerm(), _messageLog.LastLogIndex());
                //_outboundEventService.SendElection(constituent.Address);
            }
        }

        /// <summary>
        /// A log is more complete if the callers last log term is greater than the servers last log term or
        /// if the last log terms are equal but the callers last log index is greater than the servers last log index
        /// </summary>
        /// <returns></returns>
        private bool IsMoreComplete(int lastLogTerm, int lastLogIndex)
        {
            if ((lastLogTerm > _messageLog.LastLogTerm()) || (lastLogTerm == _messageLog.LastLogTerm() && lastLogIndex > _messageLog.LastLogIndex()))
            {
                return true;
            }
            return false;
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
    }
}