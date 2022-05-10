# A Sample Raft Distributed Server Implementation

This project contains my implementation of a raft distributed state-machine server. While I was not able to complete the entirety of the project, I have a handful of core concepts partially complete to show the approach to the different aspects of the objective.

Below is a simple explanation of the Raft protocol which is summarized from the Raft protocol documentation which can be found here. https://raft.github.io/raft.pdf

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

## Safety Principles

In the raft protocol safety is ensured by the following principles:

- There can only be one leader during a term. 
- A leader can never overwrite or delete log entries only append.
- If two logs contain an entry with the same index and term then the logs are identical in all entries.
- When a log entry is committed in a current term it will always be present in all future terms.
- If a node applies a log entry at a given index no other server will ever apply a different log entry for the same index.

# Implementation

## Raft.Processor

The Raft.Processor contains the core business logic of the raft protocol. There is an interface for the main entry point to the processor `INodeProcessor`. 

### Start-up 

When the raft processor is constructed it starts a heartbeat timer. When the heartbeat timer elapses the processor will issue a vote request if the server is a follower/candidate or a appendEntries/heartbeat request if the server is a leader.

### Handling Requests

The raft processor interface has four public methods. These methods are called when different requests or responses are received from the other nodes.

#### AppendEntriesReceived

The AppendEntriesReceived call will instruct a follower node to first check if the request is valid and if it is the command that was send will be added to the server’s MessageLog. 

If the request is not valid a failure message will be sent.

#### RequestVoteReceived

When a follower receives a RequestVoteReceived call it first check to ensure it hasn’t voted and that the caller is eligible to be a leader. Once those conditions are satisfied a success response will be sent back to the server.

#### AppendEntriesResponseReceived

A response to an AppendEntries request can either be a success, which indicates that the entry was accepted by the follower node or it can be a failure. If it is a failure the server will make another attempt by sending an older log entry until a common entry is found.

Once a majority of follower nodes indicate they have successfully received the appendEntries request then the server will commit the message and respond to the client with a success message.

#### RequestVotesResponseReceived

When a response to a requestVotes request is received the server will check if it is a yes vote. If so once a majority are received the server will become the leader. If it is a failure, then the server will check to see if the failure indicates that a node has a higher term. If so, the server will go back to being a follower immediately.

### Raising Events
There are two types of events the processor will raise. These events will indicate that a request needs to be made to a different server node. The events can be of two types.

#### SendVoteRequest
A send vote request is raised when a call needs to be made to another server node requesting that a vote be made by that server.

#### SendAppendEntries
A send append entries request is raised when a call needs to be made to another server node when attempting to commit a new message. This will occur when a client sends in a new command to the distributed system.
A send append entries request is also raised when the heartbeat timer elapses in the event that no client requests have come in.

## Raft.UnitTest

The unit test project contains the tests to verify the business logic of the Raft.Processor. 

The follower tests have been fully implemented. The leader and candidate tests were not completed.

## Raft.Server

The Raft.Server project is a quick attempt to provide the socket connection layer to a full Raft implementation.

The Raft.Server is a console project that starts a single server on port 8000. During the startup of the server additional servers will start up (currently only 1 additional server). This other server will be on port 8001. 

The Raft.Server is not complete. It was quickly added to illustrate the approach to demonstrating a distributed system. 

### Connecting to other Nodes

The Raft.Server will attempt to connect to other nodes whenever the Raft.Processor raises an event requesting a call be made to another server. 

### Handling requests

When the Raft.Processor raises an event indicating a call to another server should be made a request is added to a processing queue. This queue is continually checked and when a request is found a call to another server is made.

The requests are stored in the queue with the following format:

$"{serverAddress}:$:V:-:{term}:-:{lastLogTerm}:-:{lastLogIndex}"

The serverAddress is the port number of the server the request needs to go to.

The second part of the request is the type of command. In this example "V" indicates it is a voteRequest command.

The term, lastLogTerm and lastLogIndex are the call parameters that will be sent to the server as part of the request.

## Raft.Client

The Raft.Client is an implementation of a client that can issue commands to the distributed system. In this approach the client will also create an instance of the Raft.Server on port 8000. 

The port 8000 server will be the main server instance which will then generate the other server nodes. All communication will be sent from the client to this node. In a completed Raft system the client would have its requests forwarded to the leader node. This functionality is not implemented.

## Conclusions

While the implementation of the Raft protocol is incomplete it is my hope that the overall development approach can be seen from the sample work contained in this repository. The main effort was put into the follower logic of the NodeProcessor along with fully testing the follower logic. The rest was more quickly added to show the remaining work that is needed.

