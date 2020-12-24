using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Main
{
    public string ServerIP;
    public string ClientIP;

    private TCPServer Server;
    public readonly int Port = 38000;
    private readonly byte StartByte = (byte)'J';
    public void StartServer()
    {
        Server = new TCPServer(Port, startByte: StartByte);
        ServerIP = Server.SetupServer();
        ClientIP = Server.StartListener();
    }
}
