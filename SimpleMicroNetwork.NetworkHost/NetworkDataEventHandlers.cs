using SimpleMicroNetwork.NetworkData;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkHost
{
    class NetworkDataEventHandlers
    {
    }

    /// <summary>
    /// Base network delegate handler to send irecieved messages.
    /// </summary>
    /// <param name="sender">Set the class from sender.</param>
    /// <param name="e">Set the message.</param>
    public delegate void NetworkMessageEventHandler(object sender, NetworkMessageEventArgs e);

    /// <summary>
    /// network error handler. Used by exception with the network manager.
    /// </summary>
    /// <param name="sender">Set the class from sender.</param>
    /// <param name="e">Set the error message.</param>
    public delegate void NetworkMessageErrorEventHandler(object sender, NetworkMessageEventArgs e);

    /// <summary>
    /// Network handler for messenging shutdown of the socket connection.
    /// </summary>
    /// <param name="sender">Set the class from sender.</param>
    /// <param name="e">Set the message about shotdown.</param>
    public delegate void NetworkMessageShutdownEventHandler(object sender, NetworkMessageEventArgs e);

    /// <summary>
    /// Network log handler to looking step by step of using the network manager.
    /// </summary>
    /// <param name="stateMessage">Set message of doing.</param>
    public delegate void NetworkLogEventHandler(string stateMessage);
}
