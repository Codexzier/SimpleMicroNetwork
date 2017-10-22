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

        private int _countConnection = 0;

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
            this.NetworkLog("Timeout..");

            return this.State == NetworkManagerState.Running || this.State == NetworkManagerState.Waiting;
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
                    this.State = NetworkManagerState.Waiting;

                    // with accepted connection to client.
                    this.WaitConnectAndPrepareNext();
                }
                catch (Exception ex)
                {
                    this.State = NetworkManagerState.Stopped;
                    this.MessageErrorEvent(ex.Message);
                }
            }

            this.State = NetworkManagerState.Stopped;
        }

        private void WaitConnectAndPrepareNext()
        {
            Task.Run(() =>
            {
                Socket connectToClient = this.GetAcceptSocket();

                this.WaitConnectAndPrepareNext();

                this.ListenIncomingData(connectToClient);

                if (this._countConnection == 0)
                {
                    connectToClient.Shutdown(SocketShutdown.Both);
                    this.NetworkLog("a client has diconnected");
                }
            });
        }

        private Socket GetAcceptSocket()
        {
            Socket connectToClient = this._host.Accept();
            this.State = NetworkManagerState.Running;
            this._countConnection++;
            this.NetworkLog("a next client has connected");

            return connectToClient;
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

            try
            {
                int countConfirm = 0;

                while (this.State != NetworkManagerState.ToStop && connectToClient.Receive(bufferReceive, 0, bufferReceive.Length, SocketFlags.None) > 0)
                {
                    countConfirm++;
                    connectToClient.Send(this.GetMessageToBytes($"Host Received {DateTime.Now.Ticks}, Confirm Count: {countConfirm}\n"));

                    this.AllocateReceived(bufferReceive);

                    if (this.State == NetworkManagerState.ToStop)
                    {
                        break;
                    }

                    // reset buffer
                    bufferReceive = new byte[1024];
                }
            }
            catch(Exception e)
            {
                this.MessageErrorEvent(e.Message);
            }

            this._countConnection--;
        }

        private void AllocateReceived(byte[] bufferReceive)
        {
            string receivedResult = this.TranslateReceiveMessage(bufferReceive);
            this.MessageEvent(receivedResult.Replace('\0', ' ').Trim());
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
