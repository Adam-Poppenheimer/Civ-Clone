using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableHexCellData {

        #region instance fields and properties

        [DataMember()] public HexCoordinates Coordinates;
        [DataMember()] public TerrainType    Terrain;
        [DataMember()] public TerrainFeature Feature;
        [DataMember()] public int            Elevation;
        [DataMember()] public int            WaterLevel;
        [DataMember()] public bool           SuppressSlot;
        [DataMember()] public List<bool>     Roads;
        [DataMember()] public bool           HasOutgoingRiver;
        [DataMember()] public HexDirection   OutgoingRiver;
        [DataMember()] public bool           IsSlotOccupied;
        [DataMember()] public bool           IsSlotLocked;

        #endregion

    }

}
