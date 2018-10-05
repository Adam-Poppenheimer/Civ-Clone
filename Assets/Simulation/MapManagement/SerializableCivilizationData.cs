using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableCivilizationData {

        #region instance fields and properties

        [DataMember()] public string                  TemplateName;
        [DataMember()] public int                     GoldStockpile;
        [DataMember()] public int                     CultureStockpile;

        [DataMember()] public List<string>            TechQueue;
        [DataMember()] public List<string>            DiscoveredTechs;
        [DataMember()] public Dictionary<string, int> ProgressOnTechs;

        [DataMember()] public SerializableSocialPolicyData SocialPolicies;

        #endregion

    }

}
