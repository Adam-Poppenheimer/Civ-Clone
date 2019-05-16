using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableBuildingData {

        #region instance fields and properties

        [DataMember()] public HexCoordinates CityLocation;
        [DataMember()] public string         Template;
        [DataMember()] public List<bool>     IsSlotOccupied;
        [DataMember()] public List<bool>     IsSlotLocked;

        #endregion

    }

}
