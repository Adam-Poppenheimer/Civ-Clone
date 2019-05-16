using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableProjectData {

        #region instance fields and properties

        [DataMember()] public string BuildingToConstruct;

        [DataMember()] public string UnitToConstruct;

        [DataMember()] public int Progress;

        #endregion

    }

}
