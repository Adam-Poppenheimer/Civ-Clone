using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableCityData {

        #region instance fields and properties

        [DataMember()] public HexCoordinates Location;
        [DataMember()] public string         Owner;

        [DataMember()] public int Population;

        [DataMember()] public int            FoodStockpile;
        [DataMember()] public int            CultureStockpile;
        [DataMember()] public YieldFocusType YieldFocus;

        [DataMember()] public SerializableProjectData ActiveProject;
        
        [DataMember()] public int   Hitpoints;
        [DataMember()] public float CurrentMovement;

        #endregion

    }

}
