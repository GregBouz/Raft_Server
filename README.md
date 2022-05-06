# A Sample Raft Distributed Server Implementation

## Raft Protocol Rules

- All nodes have three states: follower, candidate, leader
- Only a single node can be leader at a time

### Follower

- A follower will listen for messages from candidates (`RequestVotes`) and leaders (`AppendEntries`). 
- If a follower receives a message from the client it will forward the message to the leader.
- If the follower's ElectionTimeout elapses they will become a candidate and send out a `RequestVotes` message to all other nodes.

### Candidate

- A candidate sends out `RequestVotes` commands and waits for responses from nodes.
- If a majority of nodes respond with a vote the candidate then becomes the leader.

### Leader

- A leader sends out `AppendEntries` messages when a write-request is received from the client.
- If the `HeartbeatTimeout` elapses prior to a client message being received an empty `AppendEntries` message will be sent to all nodes.

## 
