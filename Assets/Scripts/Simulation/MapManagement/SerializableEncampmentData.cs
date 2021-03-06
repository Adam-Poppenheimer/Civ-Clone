﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableEncampmentData {

        #region instance fields and properties

        [DataMember()] public HexCoordinates Location;
        [DataMember()] public int            SpawnProgress;

        #endregion

    }

}
