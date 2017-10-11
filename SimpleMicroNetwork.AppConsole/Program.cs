using SimpleMicroNetwork.NetworkClient;
using SimpleMicroNetwork.NetworkHost;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMicroNetwork.AppConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("s for Server, c for client: ");
            string optionRead = Console.ReadLine();

            switch(optionRead)
            {
                case "s":
                    {
                        NetworkManager host = new NetworkManager(1200, 10, true);

                        host.NetworkLogEvent += Host_NetworkLogEvent;
                        host.NetworkMessageEvent += Host_NetworkMessageEvent;
                        host.NetworkMessageShutdownEvent += Host_NetworkMessageShutdownEvent;

                        if (host.Start())
                        {
                            Console.WriteLine("host was startet");
                            bool isGoingToStop = false;
                            while(host.State == NetworkManagerState.Running || host.State == NetworkManagerState.Waiting)
                            {
                                if (!isGoingToStop)
                                {
                                    Console.WriteLine("s to stop host.");
                                    string setToStop = Console.ReadLine();
                                    if (setToStop == "s")
                                    {
                                        host.Stop();
                                        isGoingToStop = true;
                                    }
                                    Task.Delay(2000);
                                }
                                else
                                {
                                    Console.Write(".");
                                    Task.Delay(1000);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("host has not startet");
                        }

                        break;
                    }
                case "c":
                    {
                        NetworkManagerClient client = new NetworkManagerClient();

                        client.NetworkMessage += Client_NetworkMessage;

                        Console.WriteLine("Wait for connect");
                        string connectResult = client.Connect(1200);

                        Console.Write("Connect Result: ");
                        Console.WriteLine(connectResult);

                        while (client.IsConnected)
                        {
                            Console.Write("Enter a message for send: ");
                            string message = Console.ReadLine();
                            client.Send(Encoding.UTF8.GetBytes(message));

                            Console.Write("s to stop, nothing to enter next message: ");
                            string clientOption = Console.ReadLine();
                            if (clientOption == "s")
                            {
                                break;
                            }

                            Task.Delay(500);
                        }

                        Console.WriteLine("---------------------------");
                        break;
                    }
                default:
                    Console.WriteLine("Invalid Option. Application stop and end");
                    break;
            }
            
            Console.WriteLine("hit enter for stop application");
            string str = Console.ReadLine();
        }

        private static void Host_NetworkMessageShutdownEvent(object sender, NetworkData.NetworkMessageEventArgs e) => Console.WriteLine(e.Message);

        private static int _countMessage = 0;
        private static DateTime _lastTime;
        private static string _messagePerSec = string.Empty;
        private static void Host_NetworkMessageEvent(object sender, NetworkData.NetworkMessageEventArgs e)
        {
            DateTime dt = DateTime.Now;
            if (_lastTime == null || dt >= _lastTime.AddSeconds(1))
            {
                _messagePerSec = $"Message per Secound: {_countMessage}";

                _lastTime = DateTime.Now;
                _countMessage = 0;
            }
            else
            {
                _countMessage++;
            }

            Console.WriteLine("Received Message: " + e.Message + " (" + _messagePerSec + ")");

        }

        private static void Host_NetworkLogEvent(string stateMessage) => Console.WriteLine(stateMessage);
        private static void Client_NetworkMessage(object sender, NetworkData.NetworkMessageEventArgs e) => Console.WriteLine(e.Message);
    }
}
