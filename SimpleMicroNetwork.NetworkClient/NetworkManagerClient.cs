using SimpleMicroNetwork.NetworkData;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleMicroNetwork.NetworkClient
{
    /// <summary>
    /// Dient mehr zu Test zwecken.
    /// </summary>
    public class NetworkManagerClient
    {
        private Socket _socket = null;
        private int _timeout = 5000;
        private Thread _thread;
        private ManualResetEvent _clientDone = new ManualResetEvent(false);
        private string _message = string.Empty;
        private string _operationResult = string.Empty;

        private bool _waitForConnect = false;

        public string Connect(int port)
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Operations event Erzeugen und Verbindung herstellen, die asynchron verläuft
            this._socket.ConnectAsync(this.CreateOperationEvent(new DnsEndPoint("127.0.0.1", port)));

            // Blockiert den Thread in Millisekunden.
            this._clientDone.WaitOne(this._timeout);

            return this._operationResult;
        }

        public void Send(byte[] data)
        {
            if (this._thread == null || !this._thread.IsAlive)
            {
                this._thread = new Thread(new ParameterizedThreadStart(Send_InThread));
                this._thread.Start(data);

                int timeout = 0;
                while (!this._waitForConnect)
                {
                    Debug.WriteLine("Waiting for connecting...");
                    Thread.Sleep(1000);

                    if (timeout >= 10)
                    {
                        throw new Exception("connection Timeout");
                    }

                    timeout++;
                }
            }
        }

        private void Send_InThread(object obj)
        {
            //SocketAsyncEventArgs socketEvent = this.CreateOperationEvent(this._socket.RemoteEndPoint);
            //socketEvent.SetBuffer((byte[])obj, 0, ((byte[])obj).Length);
            //this._socket.SendAsync(socketEvent);

            int resultSend = this._socket.Send((byte[])obj);

            this._clientDone.WaitOne(this._timeout);

            this.MessageEvent("(" + this._operationResult + ") " + this._message);

            this._waitForConnect = true;
        }

        private SocketAsyncEventArgs CreateOperationEvent(EndPoint remoteEndPoint)
        {
            this._operationResult = "Operation Timeout";

            SocketAsyncEventArgs socketEvent = new SocketAsyncEventArgs();
            socketEvent.RemoteEndPoint = remoteEndPoint;
            socketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                if (e.Buffer != null)
                {
                    this._message = Encoding.ASCII.GetString(e.Buffer);
                }

                // Ruft das Ergebnis der Anfrage
                this._operationResult = e.SocketError.ToString();

                // Signal, dass der Antrag vollständig ist
                this._clientDone.Set();
            });

            // Legt den Status des Ereignisses für Kein Signal, 
            // verursacht Thread Blockade
            this._clientDone.Reset();

            return socketEvent;
        }

        public void Close()
        {
            if (this._socket != null)
            {
                //_Socket.Close();
                this._socket = null;
            }
        }

        public bool IsSending
        {
            get
            {
                return this._thread.ThreadState == System.Threading.ThreadState.Running;
            }
        }

        public bool IsConnected
        {
            get
            {
                // Das Try and Catch ist normalerweise nicht Notwendig, da es sich um ein Beispiel handelt, 
                // habe die Aufwand abgekürzt
                try
                {
                    return this._socket.Connected;
                }
                catch
                {
                    // Wenn z.B. Socket gleich Null ist
                    return false;
                }
            }
        }

        public delegate void NetworkMessageHandler(object sender, NetworkMessageEventArgs e);
        public event NetworkMessageHandler NetworkMessage;

        public virtual void MessageEvent(string message)
        {
            this.NetworkMessage?.Invoke(this, new NetworkMessageEventArgs(message));
        }

        //public delegate void NetworkMessageWasSendEventHandler(object sender, NetworkMessageEventArgs e);
        //public event NetworkMessageWasSendEventHandler NetworkMessangeWasSendEvent;

        //public virtual void MessageWasSend(string message)
        //{
        //    this.NetworkMessangeWasSendEvent?.Invoke(this, new NetworkMessageEventArgs(message));
        //}
    }
}
