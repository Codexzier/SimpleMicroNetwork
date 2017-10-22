using Newtonsoft.Json;
using SimpleMicroNetwork.NetworkData;
using System;

namespace SimpleMicroNetwork.NetworkConverter
{
    public class PrepareConverter
    {
        public string PrepareAndSerializeObject(IMicroNetworkData data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
