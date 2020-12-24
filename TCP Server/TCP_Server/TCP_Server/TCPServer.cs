using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TCPServer
{
    #region "Definitions"
    private readonly int TimeoutTime = 50;
    private readonly int BufferSize = 1024 * 64;
    private TcpListener server = null;
    public bool ServerStarted = false;
    public int HeaderLen { get; } = 5;
    private byte StartByte;
    public bool IsClientConnected = false;
    private TcpClient client;
    private int Port;
    private string IP = "";
    #endregion

    public TCPServer(int port = 38000,string ip="", int bufferSize = 1024 * 64, byte startByte = (byte)('A'))
    {
        this.Port = port;
        this.BufferSize = bufferSize;
        this.StartByte = startByte;
        this.IP = ip;
    }
    /// <summary>
    /// Setups a server with specified port and ip address(optional), if no IP is given, then this function will 
    /// automatically detect the ip address. (There is a risk of this function may open the port in ethernet card instead of wifi)
    /// </summary>
    /// <returns>returns the ip address that the server is started on</returns>
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
                        this.IP = localAddr.ToString();
                    }
                }
            }
            else
                localAddr = IPAddress.Parse(IP);
            server = new TcpListener(localAddr, Port);
            Debug.WriteLine("IP: " + localAddr + " Port: " + Port);
            server.Start();
            ServerStarted = true;
            return localAddr.ToString();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Client Failed to connect!  " + e.ToString());
            return null;
        }
    }
    /// <summary>
    /// Starts listening the port for upcoming client connection. 
    /// Warning: this function runs on blocking mode, which means it will block the thread until a client connects.
    /// </summary>
    /// <returns>Returns the ip address of the client </returns>
    public string StartListener()
    {
        try
        {
            Debug.WriteLine("Waiting for new connection...");
            client = server.AcceptTcpClient();
            IsClientConnected = true;
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            IPAddress ClientIpAddress = endPoint.Address;
            client.ReceiveBufferSize = BufferSize;
            client.SendBufferSize = BufferSize;
            Debug.WriteLine("Connected to: " + client.Client.RemoteEndPoint.ToString());
            return endPoint.Address.ToString();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Client Failed to connect!  " + e.ToString());
            return null;
        }
    }
    /// <summary>
    /// Closes the Server and disposes all the objects
    /// </summary>
    public void CloseServer()
    {
        try
        {
            if (server == null)
                return;
            server.Stop();
            server.Stop();
            Debug.WriteLine("Server has been stopped");
            ServerStarted = false;
            server = null;
        }
        catch
        {
            Debug.WriteLine("could not close the Server ");
        }
    }
    /// <summary>
    /// Sends given data to client. Adds headerBytes before data bytes to secure the communication. Refer to PrepareDataHeadermethod for further information.
    /// </summary>
    /// <param name="Data">byte array to be sent to client</param>
    /// <returns>Returns true if data is succesfully sent</returns>
    public bool SendDataToClient(byte[] Data)
    {
        byte[] headerBytes = PrepareDataHeader(Data.Length);
        byte[] dataToSend = new byte[Data.Length + HeaderLen];
        headerBytes.CopyTo(dataToSend, 0);
        Data.CopyTo(dataToSend, HeaderLen);
        bool success = false;
        try
        {
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();
                int DataLength = dataToSend.Length;
                if (DataLength <= BufferSize)
                {
                    if (stream.CanWrite)
                        stream.Write(dataToSend, 0, DataLength);
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    int numBytesRead = 0;
                    int bytesLeft = DataLength;
                    int totalBytesSent = 0;
                    int Len = BufferSize;
                    byte[] _data;
                    while (bytesLeft > 0)
                    {
                        _data = new byte[Len];
                        Array.Copy(dataToSend, totalBytesSent, _data, 0, Len);
                        numBytesRead = Len;
                        if (stream.CanWrite)
                            stream.Write(_data, 0, Len);
                        else
                        {
                            return false;
                        }
                        bytesLeft -= numBytesRead;
                        totalBytesSent += numBytesRead;
                        Len = Math.Min(bytesLeft, BufferSize);
                    }
                }
                success = true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Unable to Send Message: " + e.ToString());
            IsClientConnected = false;
            client.Close();
            client.Dispose();
        }

        return success;
    }
    /// <summary>
    /// Receives data from network stream which was sent by client. 
    /// Removes the header bytes from received data that is added before sending the package just before transmission.
    /// </summary>
    /// <returns>Pure data(without header bytes) as byte array</returns>
    public byte[] GetData()
    {
        try
        {
            var stream = client.GetStream();
            Stopwatch watchdog = new Stopwatch();
            watchdog.Restart();
            byte[] data = new byte[BufferSize];
            using (MemoryStream ms = new MemoryStream())
            {
                int numBytesRead;
                bool _isfirstSampleReceived = false;
                int totalbytesReceived = 0;
                byte[] Header = new byte[BufferSize];
                int DataLength = 0;
                while (watchdog.ElapsedMilliseconds < TimeoutTime)
                {
                    if (!_isfirstSampleReceived)
                    {
                        numBytesRead = stream.Read(data, 0, HeaderLen);
                        if (numBytesRead == HeaderLen)
                        {
                            if (data[0] != StartByte)
                                break;
                            DataLength = BitConverter.ToInt32(data, 3);
                            _isfirstSampleReceived = true;
                            watchdog.Restart();
                        }
                        else
                        {
                            Debug.WriteLine("Missing Header Bytes!");
                            break;
                        }
                    }
                    else
                    {
                        if (DataLength <= BufferSize)
                        {
                            numBytesRead = stream.Read(data, 0, DataLength);
                            totalbytesReceived += numBytesRead;
                            ms.Write(data, 0, numBytesRead);
                            watchdog.Restart();
                        }
                        else
                        {
                            int Len = BufferSize;
                            while (true)
                            {
                                numBytesRead = stream.Read(data, 0, Len);
                                watchdog.Restart();
                                ms.Write(data, 0, numBytesRead);
                                totalbytesReceived += numBytesRead;
                                Len = Math.Min(DataLength - totalbytesReceived, BufferSize);
                                if ((totalbytesReceived) >= DataLength)
                                {
                                    break;
                                }
                            }
                        }
                        if ((totalbytesReceived) >= DataLength)
                        {
                            byte[] ReceivedData = new byte[ms.Length];
                            ReceivedData = ms.ToArray();
                            if (DataLength == ReceivedData.Length)
                            {
                                return ReceivedData;
                            }
                            else
                            {
                                Debug.WriteLine("Data Length does not match! Told: " + DataLength + " But received: " + (ReceivedData.Length));
                                return null;
                            }
                        }
                    }
                }
                Debug.WriteLine("Timeout : ");
                return null;
            }
        }
        catch
        {
            Debug.WriteLine(DateTime.Now + "  :Receive Data Failed!");
            IsClientConnected = false;
            client.Close();
            client.Dispose();
            return null;
        }
    }
    /// <summary>
    /// Creates a byte array to store header bytes.
    /// Header bytes contains 1 start byte and 4 length bytes.
    /// Length bytes carries the length of bytes that starts after header bytes.
    /// Length=length of bytes to be transfered - length of header bytes
    /// </summary>
    /// <param name="dataLength">the length of byte array to be transfered</param>
    /// <returns>Data header as 5 bytes length array</returns>
    private byte[] PrepareDataHeader(int dataLength)
    {
        byte[] header = new byte[5];
        header[0] = this.StartByte;
        byte[] lengthBytes = BitConverter.GetBytes(dataLength);     /// Converts integer to byte array (Little Endian)
        lengthBytes.CopyTo(header, 1);
        return header;
    }
}
