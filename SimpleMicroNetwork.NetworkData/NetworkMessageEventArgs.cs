using System;

namespace SimpleMicroNetwork.NetworkData
{
    public class NetworkMessageEventArgs
    {
        public NetworkMessageEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
