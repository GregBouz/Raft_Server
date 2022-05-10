// See https://aka.ms/new-console-template for more information
using Raft.Processor;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

if (args.Length == 0 || string.IsNullOrEmpty(args[0]) || !int.TryParse(args[0], out int result))
{
    throw new ArgumentException("A valid port number must be provided.");
}

int port = result;

Console.WriteLine($"Starting Raft Server on Port {port}");

var serverNodes = new Dictionary<string, Constituent>();
foreach (var portNumber in new int[] { 8000, 8001 })
{
    if (port != portNumber)
    {
        serverNodes.Add(portNumber.ToString(), new Constituent() {  Address = portNumber.ToString(), NextIndex = 0 });
        // If this is the main server then start processes for the other server nodes
        if (port == 8000)
        {
            using (var serverNode1 = new Process())
            {
                serverNode1.StartInfo.FileName = @"Raft.Server.exe";
                serverNode1.StartInfo.UseShellExecute = true;
                serverNode1.StartInfo.Arguments = portNumber.ToString();
                serverNode1.Start();
            }
        }
    }
}

INodeProcessor _nodeProcessor = new NodeProcessor(port.ToString(), new NodeConfiguration()
{
    Constituents = serverNodes
});

byte[] _buffer = new byte[1024];
ConcurrentBag<Socket> _serverSockets = new ConcurrentBag<Socket>();
Queue<string> _requestQueue = new Queue<string>();
// Create a client socket to make requests to other nodes
Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
_serverSocket.Listen();
_serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

_nodeProcessor.OnVoteRequest += (sender, serverAddress, term, lastLogTerm, lastLogIndex) => {

    // Get data to send to server node
    var requestCommand = $"{serverAddress}:$:V:-:{term}:-:{lastLogTerm}:-:{lastLogIndex}";

    var serverPortNumber = int.Parse(serverAddress);
    var serverSocket = _serverSockets.FirstOrDefault(s => ((IPEndPoint)s.RemoteEndPoint)?.Port == serverPortNumber);
    if (serverSocket == null || !serverSocket.Connected)
    {
        var newServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        newServerSocket.BeginConnect(new IPEndPoint(IPAddress.Loopback, serverPortNumber), new AsyncCallback(OnConnect), newServerSocket);
        _serverSockets.Add(newServerSocket);
        _requestQueue.Enqueue(requestCommand);
    }
    _requestQueue.Enqueue(requestCommand);
};

_nodeProcessor.OnAppendEntries += (sender, receiverAddress, term, index, logEntries, lastLogTerm, lastLogIndex) => {
    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    byte[] data = Encoding.UTF8.GetBytes("Vote");
    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
};

_nodeProcessor.Start();

while (true)
{
    if(_requestQueue.Count > 0)
    {
        var requestToProcess = _requestQueue.Peek();
        SendRequest(requestToProcess);
    }
}

void OnConnect(IAsyncResult asyncResult)
{
    Socket socket = (Socket)asyncResult.AsyncState;
    _serverSockets.Add(socket);
    Console.WriteLine($"Connected to server node {socket.RemoteEndPoint}");
}

void OnAccept(IAsyncResult asyncResult)
{
    Socket socket = _serverSocket.EndAccept(asyncResult);
    _serverSockets.Add(socket);
    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
    _serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
}

void OnReceive(IAsyncResult asyncResult)
{
    Socket socket = (Socket)asyncResult.AsyncState;
    int received = socket.EndReceive(asyncResult);
    byte[] dataBuffer = new byte[received];
    Array.Copy(_buffer, dataBuffer, received);

    string text = Encoding.UTF8.GetString(dataBuffer);
    if (text.StartsWith("V"))
    {
        var parts = text.Split(":-:");
        var votesRequest = new RequestVotesRequest()
        {
            Term = int.Parse(parts[1]),
            LastLogTerm = int.Parse(parts[2]),
            LastLogIndex = int.Parse(parts[3])
        };
        var voteResponse = _nodeProcessor.RequestVoteReceived(((IPEndPoint)socket.RemoteEndPoint)?.Port.ToString(), votesRequest);
        Console.WriteLine($"Vote Response = {voteResponse.Vote}");
    }
    Console.WriteLine("Text received: " + text);
    _buffer = new byte[1024];
}

void SendRequest(string request)
{
    var parts = request.Split(":$:");
    var serverAddress = parts[0];
    var command = parts[1];

    byte[] data = Encoding.UTF8.GetBytes(command);
    var serverSocket = _serverSockets.FirstOrDefault(s => ((IPEndPoint)s.RemoteEndPoint)?.Port == int.Parse(serverAddress));
    if (serverSocket != null)
    {
        Console.WriteLine($"Sent request: {request}");
        serverSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), serverSocket);
        
        // Only remove the item from the queue if send is successful
        _requestQueue.Dequeue();
    }
}

void OnSend(IAsyncResult asyncResult)
{
    Socket socket = (Socket)asyncResult.AsyncState;
    socket.EndSend(asyncResult);
    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
}