using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;

class Main
{
    #region public variables

    public string ServerIP;
    public string ClientIP;

    public Color LedColor
    {
        get
        {
            lock (lck_LedColor)
                return _ledColor;
        }
        set
        {
            lock (lck_LedColor)
                _ledColor = value;
        }
    }
    public string ClientMessage
    {
        get
        {
            lock (lck_ClientMessage)
                return _clientMessage;
        }
        set
        {
            lock (lck_ClientMessage)
                _clientMessage = value;
        }
    }


    private Color _ledColor = Color.Black;
    private string _clientMessage = "";
    private object lck_LedColor = new object();
    private object lck_ClientMessage = new object();


    #endregion

    #region Parameters
    public readonly int Port = 38000;
    private readonly byte StartByte = (byte)'J';
    private int CommunicationFrequency = 50;        /// Hz

    #endregion

    #region Private Variables

    private TCPServer Server;
    private Thread thread_Communication;
    private double CommunicationPeriod;
    private bool ThreadEnabled = false;
    #endregion

    public void StartServer()
    {
        Server = new TCPServer(Port, startByte: StartByte);
        ServerIP = Server.SetupServer();
        ClientIP = Server.StartListener();
    }
    public void StartCommunication()
    {
        CommunicationPeriod = 1.0 / CommunicationFrequency;
        thread_Communication = new Thread(CoreFcn);
        ThreadEnabled = true;
        thread_Communication.Start();
    }
    public void StopCommunication()
    {
        if (thread_Communication == null)
            return;
        ThreadEnabled = false;
        if (thread_Communication.IsAlive)
            thread_Communication.Abort();
        thread_Communication = null;
    }
    private void CoreFcn()
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (ThreadEnabled)
        {

            while (watch.Elapsed.TotalSeconds < CommunicationPeriod) ;
            watch.Restart();
        }
    }
}
