using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableSocialPolicyData {

        #region instance fields and properties

        [DataMember()] public List<string> UnlockedTrees;
        [DataMember()] public List<string> UnlockedPolicies;

        #endregion

    }

}
