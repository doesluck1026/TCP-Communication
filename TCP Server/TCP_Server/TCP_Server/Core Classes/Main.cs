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
    public string ClientIP;

    public Color LedColor
    {
        get
        {
            lock (Lck_LedColor)
                return _ledColor;
        }
        private set
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
        private set
        {
            lock (Lck_ClientMessage)
                _clientMessage = value;
        }
    }
    public int TargetSize
    {
        get
        {
            lock (Lck_TargetSize)
                return _targetSize;
        }
        private set
        {
            lock (Lck_TargetSize)
                _targetSize = value;
        }
    }

    private Color _ledColor;
    private string _clientMessage;
    private int _targetSize;

    private object Lck_LedColor = new object();
    private object Lck_ClientMessage = new object();
    private object Lck_TargetSize = new object();
    #endregion

    #region Parameters 

    public readonly int Port = 38001;
    private readonly byte StartByte = (byte)'J';
    private int CommmunicationFrequency = 50;       /// Hz

    #endregion

    #region Private Variables

    private TCPServer Server;
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
    private readonly int Index_TargetSize = 0;
    #endregion

    #endregion
    public Main()
    {

    }
    private void StartServer()
    {
        Server = new TCPServer(port: Port, StartByte: StartByte);
        ServerIP = Server.SetupServer();
        ClientIP = Server.StartListener();
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
        Server.CloseServer();
        Server = null;
        ServerIP = "";
        ClientIP = "";
        if (CommunicationThread!=null)
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
        StartServer();
        while (ThreadEnabled)
        {
            SendClientData();
            GetClientData();

            while (watch.Elapsed.TotalSeconds<CommunicationPeriod)
            {
                Thread.Sleep(1);
            }
            watch.Restart();
        }
    }
    private byte[] PrepareDataToBeSent()
    {
        byte[] targetSizeBytes = BitConverter.GetBytes(TargetSize);
        return targetSizeBytes;
    }
    private void SendClientData()
    {
       byte[] data=  PrepareDataToBeSent();
        Server.SendDataToClient(data);
    }
    private void GetClientData()
    {
        byte[] data = Server.GetData();
        AnalyzeReceivedData(data);
    }
    private void AnalyzeReceivedData(byte[] receivedData)
    {
        byte[] ColorBytes = new byte[3];
        byte[] MessageBytes;
        Array.Copy(receivedData, Index_LedColor, ColorBytes, 0, ColorBytes.Length);
        AssignLedColor(ColorBytes);
        int LenMessage = receivedData[Index_ClientMessage] | (receivedData[Index_ClientMessage + 1] << 8);
        MessageBytes = new byte[LenMessage];
        Array.Copy(receivedData, Index_ClientMessage + 2, MessageBytes, 0, LenMessage);
        AssignMessage(MessageBytes);
    }
    private void AssignLedColor(byte[] colorBytes)
    {
        LedColor = Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
    }
    private void AssignMessage(byte[] messageBytes)
    {
        ClientMessage = Encoding.ASCII.GetString(messageBytes);
    }
}
