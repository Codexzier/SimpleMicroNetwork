using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleMicroNetwork.NetworkData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace SimpleMicroNetwork.NetworkHost.Test
{
    [TestClass]
    public class NetworkProtocolTest
    {
        /// <summary>
        /// That create an response message with basic data packet information.
        /// </summary>
        [TestMethod]
        public void NetworkProtocolCreateResponseSimpleTest()
        {
            // arrange
            // is the date valid
            bool isValid = true;
            // count of passing this data
            short passing = 1;
            // the unique id of packet
            long uid = 123;
            // the length of incoming json data.
            int receivedMessageLength = 4;
            // the datetime create from 
            DateTime dt = DateTime.Now;

            // act
            string result = NetworkProtocol.CreateResponseMessage(isValid, passing, uid, receivedMessageLength, dt);

            // assert
            string[] sa = result.Split('\r', '\n').Where(w => !string.IsNullOrEmpty(w)).Select(s => s).ToArray();

            Assert.AreEqual(sa[0], "RESPONSE: VALID");
            Assert.AreEqual(sa[1], "RECEIVED PASSING: 1");
            Assert.AreEqual(sa[2], $"RECEIVED DATETIME: {this.GetDateTimeFormat(dt)}");
            Assert.AreEqual(sa[3], "RECEIVED UID: 123");
            Assert.AreEqual(sa[4], "RECEIVED LENGTH: 4");
        }

        /// <summary>
        /// Set backlog limit to 1 to test the event message.
        /// The event send, if the backlog overflow the max backlog size.
        /// </summary>
        [TestMethod]
        public void NetworkProtocolCreateResponseBacklog()
        {
            // arrange
            NetworkBacklog backlog = new NetworkBacklog(1);
            string backlogEventMessage = string.Empty;
            backlog.BacklogFullEvent += delegate (object sender, NetworkMessageEventArgs args)
            {
                backlogEventMessage = args.Message;
            };

            // act
            for (int i = 0; i < 2; i++)
            {
                backlog.Add(new NetworkBacklogItem());
            }

            // assert
            Assert.AreEqual(backlogEventMessage, "Backlog is full!");
        }

        /// <summary>
        /// Basis datetime format for all client.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string GetDateTimeFormat(DateTime dt)
        {
            return $"{dt.Year}-{dt.Month}-{dt.Day}T{dt.Hour}:{dt.Minute}:{dt.Second}";
        }
    }
}
