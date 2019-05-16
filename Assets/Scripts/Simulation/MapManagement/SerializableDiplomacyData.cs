using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableDiplomacyData {

        #region instance fields and properties
        
        [DataMember()] public List<Tuple<string, string>>  ActiveWars               = new List<Tuple<string, string>>();
        [DataMember()] public List<SerializableProposalData>     ActiveProposals    = new List<SerializableProposalData>();
        [DataMember()] public List<SerializableOngoingDealData>  ActiveOngoingDeals = new List<SerializableOngoingDealData>();

        #endregion

    }

}
