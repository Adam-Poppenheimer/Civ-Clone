using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableOngoingDealData {

        #region instance fields and properties

        [DataMember()] public string Sender;
        [DataMember()] public string Receiver;

        [DataMember()] public List<SerializableOngoingDiplomaticExchange> ExchangesFromSender   = new List<SerializableOngoingDiplomaticExchange>();
        [DataMember()] public List<SerializableOngoingDiplomaticExchange> ExchangesFromReceiver = new List<SerializableOngoingDiplomaticExchange>();
        [DataMember()] public List<SerializableOngoingDiplomaticExchange> BilateralExchanges    = new List<SerializableOngoingDiplomaticExchange>();

        [DataMember()] public int TurnsLeft;

        #endregion

    }

}
