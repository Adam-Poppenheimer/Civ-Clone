using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableUnitData {

        #region instance fields and properties

        [DataMember()] public HexCoordinates Location;

        [DataMember()] public string Template;

        [DataMember()] public string Owner;

        [DataMember()] public float CurrentMovement;

        [DataMember()] public int Hitpoints;

        [DataMember()] public List<HexCoordinates> CurrentPath;

        [DataMember()] public bool IsSetUpToBombard;

        #endregion

    }

}
