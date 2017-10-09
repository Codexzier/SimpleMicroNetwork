using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleMicroNetwork.NetworkClient;
using SimpleMicroNetwork.NetworkData;
using SimpleMicroNetwork.NetworkData.Enums;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMicroNetwork.NetworkHost.Test
{
    [TestClass]
    public class NetworkManagerTest
    {
        /// <summary>
        /// A client to connect to the host. 
        /// TODO: Check to change the class to an interface.
        /// </summary>
        //private NetworkManagerClient _client;

        [TestInitialize]
        public void Init()
        {
            //this._client = new NetworkManagerClient();
        }

        /// <summary>
        /// Check throw invalid operation by set an unusable port number.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (InvalidNetworkPortException))]
        public void NetworkManagerWrongPortTest()
        {
            NetworkManager host = new NetworkManager(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void NetworkManagerConnectionSendTest()
        {
            // arrange 
            string sendMessage = "My Message";
            NetworkManager host = new NetworkManager(5000);
            host.DataFormat = DataFormat.TextMessages;
            string receivedResult = string.Empty;
            host.NetworkMessageEvent += delegate (object sender, NetworkMessageEventArgs args)
            {
                receivedResult = args.Message;
            };

            // act
            bool resultStart = host.Start();
            NetworkManagerState resultState = host.State;
            NetworkManagerClient client = new NetworkManagerClient();
            string connectResult = client.Connect(5000);
            client.Send(Encoding.UTF8.GetBytes(sendMessage));

            // assert
            Assert.IsTrue(resultStart, "Start result must be 'True'");
            Assert.IsTrue(resultState == NetworkManagerState.Running, "The state must be 'Running'");
            Assert.IsTrue(connectResult == "Success", "The connect methode must return 'Success'");
            Assert.IsTrue(receivedResult == sendMessage, "Received message must the same of sended message.");
        }

        /// <summary>
        /// Can not test on default unit test run.
        /// That test run only in debug modus.
        /// </summary>
        /// <returns>The main methode used task to async the running conenction.</returns>
        [Ignore]
        [TestMethod]
        public async Task NetworkManagerProtocolTest()
        {
            // arrange 
            // SensorValueRawDataModel
            string sendMessage = NetworkProtocol.CreateMessage("{\"UID\":123,\"IsValid\":true,\"TimeStamp\":\"0001-01-01T00:00:00\",\"SensorType\":1,\"RawValue\":1234}");
            NetworkManager host = new NetworkManager(5000);
            host.DataFormat = DataFormat.JsonObject;
            string resultReceived = string.Empty;
            host.NetworkMessageEvent += delegate (object sender, NetworkMessageEventArgs args)
            {
                resultReceived = args.Message;
            };
            string resultShutdown = string.Empty;
            host.NetworkMessageShutdownEvent += delegate (object sender, NetworkMessageEventArgs args)
            {
                resultShutdown = args.Message;
                throw new Exception(resultShutdown);
            };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StringBuilder sbStateMessages = new StringBuilder();
            host.NetworkLogEvent += delegate (string stateMessage)
            {
                sbStateMessages.AppendLine($"{stateMessage}, Time: {sw.ElapsedMilliseconds}ms");
            };

            NetworkManagerClient client = new NetworkManagerClient();

            // act

            bool resultStart = host.Start();
            NetworkManagerState resultState = host.State;
            string connectResult = client.Connect(5000);
            client.Send(Encoding.UTF8.GetBytes(sendMessage));

            await Task.Delay(1000);

            // assert
            sw.Stop();
            Assert.IsTrue(resultStart,  "Start result must be 'True': " + sbStateMessages.ToString());
            Assert.IsTrue(resultState == NetworkManagerState.Running, "The state must be 'Running'");
            Assert.IsTrue(connectResult == "Success", "The connect methode must return 'Success'");
            Assert.IsTrue(resultReceived == sendMessage, "Received message must the same of sended message.");
        }

        /// <summary>
        /// Can not test on default unit test run.
        /// That test run only in debug modus.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void NetworkManagerProtocolErrorTest()
        {
            // arrange 
            // SensorValueRawDataModel
            string sendMessage = NetworkProtocol.CreateMessage("Das ist kein Json objekt");
            NetworkManager host = new NetworkManager(5000);
            host.DataFormat = DataFormat.JsonObject;
            string receivedResult = string.Empty;
            host.NetworkMessageEvent += delegate (object sender, NetworkMessageEventArgs args)
            {
                receivedResult = args.Message;
            };
            NetworkManagerClient client = new NetworkManagerClient();

            // act
            bool resultStart = host.Start();
            NetworkManagerState resultState = host.State;
            string connectResult = client.Connect(5000);
            client.Send(Encoding.UTF8.GetBytes(sendMessage));

            // assert
            Assert.IsTrue(resultStart, "Start result must be 'True'");
            Assert.IsTrue(resultState == NetworkManagerState.Running, "The state must be 'Running'");
            Assert.IsTrue(connectResult == "Success", "The connect methode must return 'Success'");
            Assert.IsTrue(receivedResult == "ERROR", "The result of received message must be 'ERROR'");
        }
    }
}
