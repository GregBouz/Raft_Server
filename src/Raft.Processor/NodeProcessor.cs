namespace Raft.Processor
{
    // Delegates
    public delegate void SendVoteRequest(string sender, string receiverAddress, int term, int lastLogTerm, int lastLogIndex);
    public delegate void SendAppendEntries(string sender, string receiverAddress, int term, int index, List<LogEntry> logEntries, int lastLogTerm, int lastLogIndex);

    /// <summary>
    /// TODO:
    /// Add functions for handling recieved events (heartbeat, voterequest, appendMessage)
    /// </summary>
    public class NodeProcessor : INodeProcessor
    { 
        private NodeConfiguration _nodeConfiguration;

        private Timer _heartbeatTimer;
        private int _heartbeatTimeoutInMilliSeconds = new Random().Next(1500, 3000);
        
        private MessageLog _messageLog;

        private bool _hasVoted = false;

        public event SendVoteRequest OnVoteRequest;
        public event SendAppendEntries OnAppendEntries;

        public Role Role { get; private set; }
        public int CurrentTerm { get; private set; }
        public int VotesReceived { get; private set; }
        public string ServerId { get; private set; }

        /// <summary>
        /// Initialize the node
        /// </summary>
        public NodeProcessor(string serverId, NodeConfiguration nodeConfiguration, TestingPresets testingPresets = null)
        {
            ServerId = serverId;
            CurrentTerm = testingPresets?.CurrentTerm != null ? testingPresets.CurrentTerm : 0;
            VotesReceived = 0;

            _messageLog = testingPresets?.MessageLog != null ? testingPresets.MessageLog : new MessageLog();
            _nodeConfiguration = nodeConfiguration;
        }

        /// <summary>
        /// Starts the server processing from the follower state.
        /// </summary>
        /// <param name="enableTimers">Used to disable the timer for testing purposes.</param>
        public void Start(bool enableTimers = true)
        {
            Role = Role.Follower;
            if (enableTimers)
            {
                _heartbeatTimer = new Timer(HandleHeartbeatTimeout, null, _heartbeatTimeoutInMilliSeconds, Timeout.Infinite);
            }
        }
        
        public void Stop()
        {
            _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public AppendEntriesResponse AppendEntriesReceived(string sender, AppendEntriesRequest request)
        {
            var response = new AppendEntriesResponse();

            // Check for log completeness from the appendEntries request
            if (request.Term < CurrentTerm)
            {
                response.Success = false;
                response.Term = CurrentTerm;
            }
            else if (request.prevLogIndex == _messageLog.LastLogIndex())
            {
                var lastLogEntry = _messageLog.LastLogEntry();
                if (request.prevLogTerm == lastLogEntry.Term)
                {
                    var newEntry = request.LogEntries.FirstOrDefault(e => e.Index == request.LeaderCommit);
                    if (newEntry != null)
                    {
                        _messageLog.AddLogEntry(request.Term, request.LeaderCommit, newEntry.Command);
                        response.Success = true;
                    }
                    else
                    {
                        response.Success = false;
                    }
                }
                // If the entries don't match delete the entry and all that follow it
                else
                {
                    response.Success = false;
                }
            }
            if (request.LeaderCommit > _messageLog.CommitIndex)
            {
                _messageLog.CommitIndex = Math.Min(request.LeaderCommit, _messageLog.LastLogIndex());
            }
            return response;
        }

        public void AppendEntriesResponseReceived(string sender, AppendEntriesResponse request)
        {
            //_messageLog.AddLogEntry(request.Term, request.Message);
        }

        /// <summary>
        /// If a vote is received update the votes received list until a majority is counted
        /// </summary>
        /// <param name="state"></param>
        public void RequestVotesResponseReceived(string sender, bool voteGranted, int term)
        {
            // If no longer leader then ignore the received vote
            if (Role == Role.Leader && voteGranted)
            {
                VotesReceived++;
                if (VotesReceived > ((_nodeConfiguration.Constituents.Count + 1) / 2))
                {
                    Role = Role.Leader;
                    IssueAppendEntries(true);
                }
            }
            else if (term > CurrentTerm)
            {
                Role = Role.Follower;
                CurrentTerm = term;
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
        public RequestVotesResponse RequestVoteReceived(string sender, RequestVotesRequest request)
        {
            ResetElectionTimeout();
            var voteResponse = false;
            if ((request.Term >= CurrentTerm) && !_hasVoted && IsMoreComplete(request.LastLogTerm, request.LastLogIndex))
            {
                _hasVoted = true;
                voteResponse = true;
            }

            // Always reset current term if a request is received with a higher term
            if (request.Term > CurrentTerm)
            {
                CurrentTerm = request.Term;
            }
            return new RequestVotesResponse() { Vote = voteResponse, CurrentTerm = CurrentTerm };
        }

        /// <summary>
        /// When the election timeout elapses: 
        /// 1) the server becomes a candidate
        /// 2) the current term is incremented
        /// 3) the server votes for itself
        /// 4) voteRequests are made to all other servers
        /// </summary>
        /// <param name="state"></param>
        private void HandleHeartbeatTimeout(object? state)
        {
            // Even after stopping the timer and disposing of it this handler can still fire
            if (_heartbeatTimer == null)
                return;

            if (Role == Role.Follower)
            {
                Role = Role.Candidate;
                StartElection();
            }
            else if (Role == Role.Candidate)
            {
                // If the timeout elapses as a candidate then start a new term and restart the election process
                StartElection();
            }
            else if (Role == Role.Leader)
            {

            }
            // Reset timer
            ResetElectionTimeout();
        }

        /// <summary>
        /// Resets the election timeout when a message from a candidate or a leader is received
        /// </summary>
        private void ResetElectionTimeout()
        {
            _heartbeatTimer.Change(_heartbeatTimeoutInMilliSeconds, Timeout.Infinite);
        }

        private void StartElection()
        {
            CurrentTerm++;
            VotesReceived = 1;
            foreach (Constituent constituent in _nodeConfiguration.Constituents.Values)
            {
                OnVoteRequest.Invoke(ServerId, constituent.Address, CurrentTerm, _messageLog.LastLogTerm(), _messageLog.LastLogIndex());
            }
        }

        private void IssueAppendEntries(bool heartbeatOnly)
        {
            foreach (Constituent constituent in _nodeConfiguration.Constituents.Values)
            {
                List<LogEntry> logEntriesToSend = new List<LogEntry>();
                if (!heartbeatOnly)
                {
                    // TODO: send log entries to constituents last log index
                }
                OnAppendEntries.Invoke(ServerId, constituent.Address, CurrentTerm, -1, logEntriesToSend, _messageLog.LastLogTerm(), _messageLog.LastLogTerm());
            }
        }

        /// <summary>
        /// A log is more complete if the callers last log term is greater than the servers last log term or
        /// if the last log terms are equal but the callers last log index is greater than the servers last log index
        /// </summary>
        /// <returns></returns>
        private bool IsMoreComplete(int lastLogTerm, int lastLogIndex)
        {
            if ((lastLogTerm > _messageLog.LastLogTerm()) || (lastLogTerm == _messageLog.LastLogTerm() && lastLogIndex >= _messageLog.LastLogIndex()))
            {
                return true;
            }
            return false;
        }
    }
}