using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkData
{
    public interface IMicroNetworkData
    {
        /// <summary>
        /// Get the Address or name of the receiver
        /// </summary>
        string Receiver { get; }

        /// <summary>
        /// Get the Address or name of the sender
        /// </summary>
        string Sender { get; }
    }
}
