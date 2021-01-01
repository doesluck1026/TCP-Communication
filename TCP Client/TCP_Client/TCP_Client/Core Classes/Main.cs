using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

class Main
{
    #region Public Variables
    public string ServerIP;
    public bool IsConnectedToServer
    {
        get
        {
            lock (Lck_IsConnectedToServer)
                return _isConnectedToServer;
        }
        private set
        {
            lock (Lck_IsConnectedToServer)
                _isConnectedToServer = value;
        }
    }
    public Color LedColor
    {
         get
        {
            lock (Lck_LedColor)
                return _ledColor;
        }
        set
        {
            lock (Lck_LedColor)
                _ledColor = value;
        }
    }
    public string ClientMessage
    {
        get
        {
            lock (Lck_ClientMessage)
                return _clientMessage;
        }
         set
        {
            lock (Lck_ClientMessage)
                _clientMessage = value;
        }
    }
    public int TargetPosition
    {
        get
        {
            lock (Lck_TargetPosition)
                return _targetPosition;
        }
        private set
        {
            lock (Lck_TargetPosition)
                _targetPosition = value;
        }
    }

    private Color _ledColor;
    private string _clientMessage;
    private int _targetPosition;
    private bool _isConnectedToServer;

    private object Lck_LedColor = new object();
    private object Lck_ClientMessage = new object();
    private object Lck_TargetPosition = new object();
    private object Lck_IsConnectedToServer= new object();

    #endregion

    #region Parameters 

    public readonly int Port = 38001;
    private readonly byte StartByte = (byte)'J';
    private int CommmunicationFrequency = 50;       /// Hz

    #endregion

    #region Private Variables

    private TCPClient Client;
    private Thread CommunicationThread;
    private double CommunicationPeriod;
    private bool ThreadEnabled = false;
    #endregion

    #region Indexes

    #region Client To Server
    private readonly int Index_LedColor = 0;
    private readonly int Index_ClientMessage = 3;
    #endregion
    #region Server To Client
    private readonly int Index_TargetPosition = 0;
    #endregion

    #endregion
    public Main(string ip)
    {
        ServerIP = ip;
    }
    private void ConnectToServer()
    {
        Client = new TCPClient(port: Port, ip: ServerIP, StartByte: StartByte);
        ServerIP = Client.ConnectToServer();
    }
    public void StartCommunicationThread()
    {
        CommunicationPeriod = 1.0 / CommmunicationFrequency;
        CommunicationThread = new Thread(CoreFcn);
        ThreadEnabled = true;
        CommunicationThread.Start();
    }
    public void StopCommunication()
    {
        Client.DisconnectFromServer();
        Client = null;
        ServerIP = "";
        if (CommunicationThread != null)
        {
            ThreadEnabled = false;
            if (CommunicationThread.IsAlive)
                CommunicationThread.Abort();
            CommunicationThread = null;
        }

    }
    private void CoreFcn()
    {
        Stopwatch watch = Stopwatch.StartNew();
        ConnectToServer();
        while (ThreadEnabled)
        {
            SendServerData();
            GetServerData();
            IsConnectedToServer = Client.IsConnectedToServer;
            while (watch.Elapsed.TotalSeconds < CommunicationPeriod)
            {
                Thread.Sleep(1);
            }
            watch.Restart();
        }
    }
    private byte[] PrepareDataToBeSent()
    {
        byte[] ColorBytes = PrepareColorBytes();
        byte[] messageBytes = PrepareMessageBytes();
        byte[] datatoSend = new byte[ColorBytes.Length + messageBytes.Length];
        ColorBytes.CopyTo(datatoSend, Index_LedColor);
        messageBytes.CopyTo(datatoSend, Index_ClientMessage) ;
        return datatoSend;
    }
    private byte[] PrepareColorBytes()
    {
        byte[] colorBytes = new byte[3];
        colorBytes[0] = LedColor.R;
        colorBytes[1] = LedColor.G;
        colorBytes[2] = LedColor.B;
        return colorBytes;
    }
    private byte[] PrepareMessageBytes()
    {
        int messageLen;
        byte[] messageData;
        if (string.IsNullOrEmpty(ClientMessage))
        {
            messageLen = 0;
            messageData = new byte[messageLen + 2];
        }
        else
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(ClientMessage);
            messageLen = messageBytes.Length;
            messageData = new byte[messageLen + 2];
            messageBytes.CopyTo(messageData, 2);
        }
        messageData[0] = (byte)(messageLen & 0xff);
        messageData[1] = (byte)((messageLen >> 8) & 0xff);

        return messageData;
    }
    private void SendServerData()
    {
        byte[] data = PrepareDataToBeSent();
        Client.SendDataServer(data);
    }
    private void GetServerData()
    {
        byte[] data = Client.GetData();
        if (data != null)
            AnalyzeReceivedData(data);
        else
            IsConnectedToServer = false;
    }
    private void AnalyzeReceivedData(byte[] receivedData)
    {
        byte[] PositionBytes = new byte[4];
        Array.Copy(receivedData, Index_TargetPosition, PositionBytes, 0, PositionBytes.Length);
        AssignPositionData(PositionBytes);
    }
    private void AssignPositionData(byte[] positionBytes)
    {
        TargetPosition = BitConverter.ToInt32(positionBytes, 0);
    }
}
