using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkHost
{
    public enum NetworkManagerState
    {
        Running,
        Stopped,
        Waiting,
        ToStop,
        Prepare
    }
}
