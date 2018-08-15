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

        [DataMember()] public HexCoordinates   Coordinates;
        [DataMember()] public CellTerrain      Terrain;
        [DataMember()] public CellVegetation   Vegetation;
        [DataMember()] public CellShape        Shape;
        [DataMember()] public CellFeature      Feature;
        [DataMember()] public bool             SuppressSlot;
        [DataMember()] public bool             HasRoads;
        [DataMember()] public bool             IsSlotOccupied;
        [DataMember()] public bool             IsSlotLocked;
        [DataMember()] public List<bool>       HasRiverAtEdge         = new List<bool>(new bool[6]);
        [DataMember()] public List<RiverFlow>  DirectionOfRiverAtEdge = new List<RiverFlow>(new RiverFlow[6]);

        #endregion

    }

}
