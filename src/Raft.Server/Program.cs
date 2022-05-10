// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Hello, World!");

if (args.Length == 0 || string.IsNullOrEmpty(args[0]) || !int.TryParse(args[0], out int result))
{
    throw new ArgumentException("A valid port number must be provided.");
}

int port = result;
byte[] _buffer = new byte[1024];
List<Socket> _clientSockets = new List<Socket>();
Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
_serverSocket.Listen();
_serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
Console.ReadLine();

void OnAccept(IAsyncResult asyncResult)
{
    Socket socket = _serverSocket.EndAccept(asyncResult);
    _clientSockets.Add(socket);
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
    Console.WriteLine("Test received: " + text);

    byte[] data = Encoding.UTF8.GetBytes(text);
    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
}

static void OnSend(IAsyncResult asyncResult)
{
    Socket socket = (Socket)asyncResult.AsyncState;
    socket.EndSend(asyncResult);
}