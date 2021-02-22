using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Core.Serialization
{
    public interface ISerializer
    {

        string Serialize(object target);
        T Deserialize<T>(string data);

    }
}
