using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableOngoingDiplomaticExchange {

        #region instance fields and properties

        [DataMember()] public ExchangeType Type;

        [DataMember()] public string Sender;
        [DataMember()] public string Receiver;

        [DataMember()] public string ResourceInput;
        [DataMember()] public int    IntInput;

        #endregion

    }

}
