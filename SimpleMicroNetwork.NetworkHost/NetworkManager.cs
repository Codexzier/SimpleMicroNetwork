using SimpleMicroNetwork.NetworkData;
using SimpleMicroNetwork.NetworkData.Enums;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleMicroNetwork.NetworkHost
{
    /// <summary>
    /// Main class of network connection with clients.
    /// </summary>
    public class NetworkManager
    {
        private NetworkBacklog _backlog;
        private Socket _host;
        private int _port;

        private CancellationToken _cancellationToken;
        private Task _runningHost;

        private bool _networkLog = false;

        /// <summary>
        /// Standard constructor with basic setup
        /// </summary>
        /// <param name="port"></param>
        /// <param name="maxBacklog"></param>
        /// <param name="logEventOn">Set log event on.</param>
        public NetworkManager(int port, int maxBacklog = 1000, bool logEventOn = false)
        {
            if (port <= 0)
            {
                throw new InvalidNetworkPortException("Wrong Portnumber!");
            }

            this._port = port;
            this._backlog = new NetworkBacklog(maxBacklog);
            this._networkLog = logEventOn;
        }

        /// <summary>
        /// Ruft den Status ab oder legt diesen fest.
        /// </summary>
        public NetworkManagerState State { get; private set; }

        /// <summary>
        /// Ruft das verwendete Format ab oder legt diese fest.
        /// Validiert den Inhalt des Empfang des eingestellten Formates.
        /// </summary>
        public DataFormat DataFormat { get; set; }

        /// <summary>
        /// Startet den Task für das Zuhören nach eingehenden Verbindungen.
        /// </summary>
        /// <returns>Wenn der Task/Thread erfolgreich gestartet werden konnte.</returns>
        public bool Start()
        {
            this.State = NetworkManagerState.Stopped;
            this.NetworkLog("State has set");
            this._cancellationToken = new CancellationToken(false);
            this.NetworkLog("cacellation token has set");
            this._runningHost = Task.Run(() => this.RunningHost(), this._cancellationToken);
            this.NetworkLog("memberfiled runningHost was set.");

            this.StartTimeout();
            this.NetworkLog("Timeout was run");

            return this.State == NetworkManagerState.Running;
        }

        /// <summary>
        /// Stop the connection.
        /// </summary>
        public void Stop()
        {
            this.State = NetworkManagerState.ToStop;
        }

        /// <summary>
        /// Einfache Verzögerung, um auf den Start des Host abzuwarten.
        /// </summary>
        private void StartTimeout()
        {
            for (int timeout = 0; timeout < 10; timeout++)
            {
                if (this.State == NetworkManagerState.Running)
                {
                    break;
                }
                
                Task.Delay(500).Wait();
            }
        }

        /// <summary>
        /// Running while the connection to the client.
        /// </summary>
        private void RunningHost()
        {
            // prepare the socket for host setup
            this.InitializeNetwork();
            
            while (this.State == NetworkManagerState.Prepare || this.State == NetworkManagerState.Running)
            {
                this.NetworkLog("while was start");
                
                try
                {
                    // wait to return the socket 
                    // with accepted connection to client.
                    Socket connectToClient = this._host.Accept();
                    this.State = NetworkManagerState.Running;
                    this.NetworkLog("state was set to 'Running'");

                    this.ListenIncomingData(connectToClient);

                    connectToClient.Shutdown(SocketShutdown.Both);
                    this.MessageShutdownEvent("connect to client has shutdown");
                }
                catch (Exception ex)
                {
                    this.State = NetworkManagerState.Stopped;
                    this.MessageErrorEvent(ex.Message);
                }
            }

            this.State = NetworkManagerState.Stopped;
        }

        /// <summary>
        /// Prepare the socket connection for tcp and listen any ip address.
        /// </summary>
        private void InitializeNetwork()
        {
            this.State = NetworkManagerState.Prepare;
            this.NetworkLog("State has set to 'ToRun'");

            this._host = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.NetworkLog("host was set");

            this._host.Bind(new IPEndPoint(IPAddress.Any, this._port));
            this.NetworkLog("bind ip and port was set");

            this._host.Listen(this._backlog.MaxBacklog);
            this.NetworkLog("backlog was set");
        }
        
        /// <summary>
        /// Listen to incoming data with the instance of the socket.
        /// This is main methode to receive data and send a event.
        /// </summary>
        /// <param name="connectToClient">Need the insance of the connected socket to the client.</param>
        private void ListenIncomingData(Socket connectToClient)
        {
            byte[] bufferReceive = new byte[1024];
            this.NetworkLog("buffer was set");

            while (connectToClient.Receive(bufferReceive, 0, bufferReceive.Length, SocketFlags.None) > 0)
            {
                connectToClient.Send(this.GetMessageToBytes("Has Received"));
                string receivedResult = this.TranslateReceiveMessage(bufferReceive);
                this.MessageEvent(receivedResult.Replace('\0', ' ').Trim());

                if(this.State == NetworkManagerState.ToStop)
                {
                    break;
                }

                // reset buffer
                bufferReceive = new byte[1024];
            }
        }

        /// <summary>
        /// Encoding the string to a byte array to UTF8 format for sending the data to the client.
        /// </summary>
        /// <param name="message">String message to transform an byte array.</param>
        /// <returns>Return the result.</returns>
        protected virtual byte[] GetMessageToBytes(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        /// <summary>
        /// Encoding the received byte array to transform with UTF8 format.
        /// </summary>
        /// <param name="receiveBytes">Received bytes to transform.</param>
        /// <returns>Return the result.</returns>
        protected virtual string TranslateReceiveMessage(byte[] receiveBytes)
        {
            return Encoding.UTF8.GetString(receiveBytes);
        }

        public event NetworkMessageEventHandler NetworkMessageEvent;
        protected virtual void MessageEvent(string message)
        {
            this.NetworkMessageEvent?.Invoke(this, new NetworkMessageEventArgs(message));
        }

        public event NetworkMessageErrorEventHandler NetworkMessageErrorEvent;
        protected virtual void MessageErrorEvent(string message)
        {
            this.NetworkMessageErrorEvent?.Invoke(this, new NetworkMessageEventArgs(message));
        }

        public event NetworkMessageShutdownEventHandler NetworkMessageShutdownEvent;
        protected virtual void MessageShutdownEvent(string message)
        {
            this.NetworkMessageShutdownEvent?.Invoke(this, new NetworkMessageEventArgs(message));
        }

        public event NetworkLogEventHandler NetworkLogEvent;
        public void NetworkLog(string stateMessage)
        {
            if (this._networkLog)
            {
                this.NetworkLogEvent?.Invoke(stateMessage);
            }
        }

        /// <summary>
        /// Clean up all open instance.
        /// </summary>
        public void Dispose()
        {
            if (this._runningHost != null)
            {
                this._runningHost = null;
            }
        }
    }

   
    
}
