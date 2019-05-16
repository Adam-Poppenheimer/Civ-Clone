using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableProposalData {

        [DataMember()] public string Sender;
        [DataMember()] public string Receiver;

        [DataMember()] public List<SerializableDiplomaticExchangeData> OfferedBySender    = new List<SerializableDiplomaticExchangeData>();
        [DataMember()] public List<SerializableDiplomaticExchangeData> DemandedOfReceiver = new List<SerializableDiplomaticExchangeData>();
        [DataMember()] public List<SerializableDiplomaticExchangeData> BilateralExchanges = new List<SerializableDiplomaticExchangeData>();

    }

}
