using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkData
{
    public class DataPackage : IMicroNetworkData
    {
        public string Receiver { set; get; }

        public string Sender { set; get; }
    }
}
