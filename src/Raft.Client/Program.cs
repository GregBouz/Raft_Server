// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

ConnectToServer();
var exit = false;
while (!exit)
{
    Console.WriteLine("Enter a command (type exit to quit): ");
    string command = Console.ReadLine();
    if (command.ToLower() == "exit")
    {
        exit = true;
    }
    Send(command);
    WaitForResponse();
}

void ConnectToServer()
{
    int attempts = 0;
    while (!_clientSocket.Connected)
    {
        try
        {
            attempts++;
            _clientSocket.Connect(IPAddress.Loopback, 8000);
        }
        catch (SocketException e)
        {
            Console.Clear();
            Console.WriteLine("Connection attempts: " + attempts.ToString());
        }
    }

    Console.WriteLine("Client Connected");
}

void WaitForResponse()
{
    var buffer = new byte[2048];
    int received = _clientSocket.Receive(buffer, SocketFlags.None);
    if (received == 0) return;
    var data = new byte[received];
    Array.Copy(buffer, data, received);
    string text = Encoding.ASCII.GetString(data);
    Console.WriteLine(text);
}

void Send(string command)
{
    byte[] buffer = Encoding.UTF8.GetBytes(command);
    _clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
}
