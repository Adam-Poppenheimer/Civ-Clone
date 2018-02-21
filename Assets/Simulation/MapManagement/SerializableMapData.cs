using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableMapData {

        #region instance fields and properties

        [DataMember()] public List<SerializableHexCellData>      HexCells;
        [DataMember()] public List<SerializableCivilizationData> Civilizations;
        [DataMember()] public List<SerializableCityData>         Cities;
        [DataMember()] public List<SerializableUnitData>         Units;
        [DataMember()] public List<SerializableImprovementData>  Improvements;
        [DataMember()] public List<SerializableResourceNodeData> ResourceNodes;
        [DataMember()] public string                             ActiveCivilization;

        #endregion

    }

}
