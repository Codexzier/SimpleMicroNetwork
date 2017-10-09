using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkHost
{
    public class InvalidNetworkPortException : Exception
    {
        public InvalidNetworkPortException(string message) : base(message)
        {
        }
    }
}
