using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Core.Serialization
{
    public class JsonSerializer : ISerializer
    {

        public JsonSerializer() { }

        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public string Serialize(object target)
        {
            return JsonConvert.SerializeObject(target);
        }


    }
}
