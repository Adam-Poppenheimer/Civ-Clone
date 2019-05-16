using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializablePromotionTreeData {

        #region instance fields and properties

        [DataMember()] public string       Template;
        [DataMember()] public List<string> ChosenPromotions;

        #endregion

    }

}
