﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableMapData {

        #region instance fields and properties

        [DataMember()] public int CellCountX;
        [DataMember()] public int CellCountZ;

        [DataMember()] public List<SerializableHexCellData>      HexCells;
        [DataMember()] public List<SerializableCivilizationData> Civilizations;
        [DataMember()] public List<SerializablePlayerData>       Players;
        [DataMember()] public List<SerializableCityData>         Cities;
        [DataMember()] public List<SerializableBuildingData>     Buildings;
        [DataMember()] public List<SerializableUnitData>         Units;
        [DataMember()] public List<SerializableImprovementData>  Improvements;
        [DataMember()] public List<SerializableResourceNodeData> ResourceNodes;
        [DataMember()] public SerializableDiplomacyData          DiplomacyData;
        [DataMember()] public string                             ActivePlayer;
        [DataMember()] public List<UniRx.Tuple<string, string>>  CivDiscoveryPairs;
        [DataMember()] public List<SerializableEncampmentData>   Encampments;

        #endregion

    }

}
