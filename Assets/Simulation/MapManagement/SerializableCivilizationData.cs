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

        [DataMember()] public string                  Name;
        [DataMember()] public SerializableColor       Color;
        [DataMember()] public int                     GoldStockpile;
        [DataMember()] public int                     CultureStockpile;

        [DataMember()] public List<string>            TechQueue       = new List<string>();
        [DataMember()] public List<string>            DiscoveredTechs = new List<string>();
        [DataMember()] public Dictionary<string, int> ProgressOnTechs = new Dictionary<string, int>();

        #endregion

    }

}
