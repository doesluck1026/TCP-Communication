using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TCPServer
{
    private TcpListener Listener;
    private TcpClient Client;

    private int Port;
    private string IP;
    private byte StartByte;
    private int BufferSize;
    public TCPServer(int port=38000,string ip"",int bufferSize=1024*64, byte StartByte=(byte)'A')
    {
        this.Port = port;
        this.IP = ip;
        this.BufferSize=bufferSize
    }
}
