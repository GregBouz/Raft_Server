namespace Raft.Processor
{
    public interface INodeProcessor
    {
        /// <summary>
        /// An event to be raised when a VoteRequest call is to be made
        /// </summary>
        event SendVoteRequest OnVoteRequest;

        /// <summary>
        /// Event raised when an AppendEntries call is to be made
        /// </summary>
        event SendAppendEntries OnAppendEntries;

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="enableTimers">Use false for testing purposes to disable elapsed timer callbacks.</param>
        void Start(bool enableTimers = true);
        
        /// <summary>
        /// Stops the server and disposes of all timers
        /// </summary>
        void Stop();

        /// <summary>
        /// Processes AppendEntries requests from servers. A response to be sent back to the server will be returned.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        AppendEntriesResponse AppendEntriesReceived(string sender, AppendEntriesRequest request);
        
        /// <summary>
        /// Processes RequestVote request from servers. A response to be sent back to the server will be returned.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        RequestVotesResponse RequestVoteReceived(string sender, RequestVotesRequest request);

        /// <summary>
        /// Processes the server response for an AppendEntries call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void AppendEntriesResponseReceived(string sender, AppendEntriesResponse request);
        
        /// <summary>
        /// Processes the server response for a RequestVote call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="voteGranted"></param>
        /// <param name="term"></param>
        void RequestVotesResponseReceived(string sender, bool voteGranted, int term);
    }
}
