using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class TCPServer
{
    #region Definitions
    private int BufferSize;
    private TcpListener Listener;
    private TcpClient Client;
    private int Port;
    private string IP;
    private byte StartByte;
    public bool IsCLientConnected = false;
    #endregion

    public TCPServer(int port = 38000, string ip = "", int bufferSize = 1024 * 64, byte StartByte =(byte)'A')
    {
        this.Port = port;
        this.IP = ip;
        this.BufferSize = bufferSize;
        this.StartByte = StartByte;
    }
    public string SetupServer()
    {
        try
        {
            IPAddress localAddr = null;
            if (string.IsNullOrEmpty(IP))
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localAddr = ip;
                    }
                }
            }
            else
            {
                localAddr = IPAddress.Parse(IP);
            }
            Listener = new TcpListener(localAddr, Port);
            Listener.Start();
            IP =Listener.LocalEndpoint.ToString();
            Debug.WriteLine("Server is ready");
            return localAddr.ToString();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to Start Server!" +e.ToString());
            return null;
        }
    }
    public string StartListener()
    {
        try
        {
            if (Listener == null)
                return null;
            Debug.WriteLine("Listener is Started IP:  " + IP + "  Port: " + Port);
            Client = Listener.AcceptTcpClient();        /// this Line is Blocking
            IsCLientConnected = true;
            IPEndPoint endPoint = (IPEndPoint)Client.Client.RemoteEndPoint;
            var ipAddress = endPoint.Address;
            Client.ReceiveBufferSize = BufferSize;
            Client.SendBufferSize = BufferSize;
            Debug.WriteLine(ipAddress + " is connected");
            return ipAddress.ToString();
        }
        catch
        {
            return null;
        }
    }
    public void CloseServer()
    {
        try
        {
            if(Client!=null)
            {
                Client.Close();
                Client = null;
            }
            if (Listener == null)
                return;
            Listener.Stop();
            IsCLientConnected = false;
            Listener = null;
        }
        catch(Exception e)
        {
            Debug.WriteLine("Failed to Stop server! : " + e.ToString());
        }
    }
    public bool SendDataToClient(byte[] data)
    {
        bool success = false;
        byte[] headerBytes = PrepareDataHeader(data.Length);
        int DataLength = headerBytes.Length + data.Length;
        byte[] dataToSend = new byte[DataLength];
        headerBytes.CopyTo(dataToSend, 0);
        data.CopyTo(dataToSend, headerBytes.Length);
        try
        {
            if (Client == null)
                return false;
            if (Client.Connected)
            {
                NetworkStream stream = Client.GetStream();
                if (DataLength < BufferSize)
                {
                    stream.Write(dataToSend, 0, DataLength);
                    success = true;
                }
                else
                {
                    int NumBytesLeft = DataLength;
                    int TotalBytesSent = 0;
                    byte[] tempData;
                    int len = BufferSize;
                    while (NumBytesLeft > 0)
                    {
                        tempData = new byte[len];
                        Array.Copy(dataToSend, TotalBytesSent, tempData, 0, len);
                        stream.Write(tempData, 0, len);
                        NumBytesLeft -= len;
                        TotalBytesSent += len;
                        len = Math.Min(NumBytesLeft, BufferSize);
                    }
                    success = true;
                }
            }
            else 
                success = false;
            return success;
        }
        catch(Exception e)
        {
            Debug.WriteLine("Unable to send message to client!" + e.ToString());
            IsCLientConnected = false;
            Client.Close();
            Client = null;
            var t = Task.Run(() => StartListener());
            return false;
        }
    }
    public byte[] GetData()
    {
        try
        {
            if (Client == null)
            {
                return null;
            }

            NetworkStream stream = Client.GetStream();
            byte[] tempData = new byte[BufferSize];
            byte[] dataHeader = new byte[5];
            using (MemoryStream ms= new MemoryStream())
            {
                int numBytesRead = 0;
                int TotalBytesReceived = 0;
                bool isFirstsSampleReceived = false;
                int DataLength = 0;
                while(true)
                {
                    if(!isFirstsSampleReceived)
                    {
                        numBytesRead = stream.Read(dataHeader, 0, dataHeader.Length);
                        if (numBytesRead == dataHeader.Length)
                        {
                            if (dataHeader[0] != StartByte)
                                break;
                            DataLength = BitConverter.ToInt32(dataHeader, 1);
                            isFirstsSampleReceived = true;
                        }
                        else
                            break;
                    }
                    else
                    {
                        if(DataLength<BufferSize)
                        {
                            numBytesRead = stream.Read(tempData, 0, DataLength);
                            TotalBytesReceived += numBytesRead;
                            ms.Write(tempData, 0, numBytesRead);
                        }
                        else
                        {
                            int len = BufferSize;
                            while(TotalBytesReceived<DataLength)
                            {
                                numBytesRead = stream.Read(tempData, 0, len);
                                TotalBytesReceived += numBytesRead;
                                ms.Write(tempData, 0, numBytesRead);
                                len = Math.Min(DataLength - TotalBytesReceived, BufferSize);
                            }
                        }
                    }
                    if (TotalBytesReceived >= DataLength)
                        break;
                }
                if (TotalBytesReceived == DataLength)
                {
                    byte[] receivedData = ms.ToArray();
                    return receivedData;
                }
                else
                {
                    Debug.WriteLine("number of received bytes are incorrect");
                    return null;
                }
            }
        }
        catch(Exception e)
        {
            Debug.WriteLine("Failed to Receive Data From Client! :" + e.ToString());
            IsCLientConnected = false;
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
            var t = Task.Run(() => StartListener());
            return null;
        }
    }
    private byte[] PrepareDataHeader(int len)
    {
        byte[] header = new byte[5];
        header[0] = StartByte;
        byte[] lengthBytes = BitConverter.GetBytes(len);
        lengthBytes.CopyTo(header, 1);
        return header;
    }
}
