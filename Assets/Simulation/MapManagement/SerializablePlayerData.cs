using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializablePlayerData {

        #region instance fields and properties

        [DataMember()] public string ControlledCiv;
        [DataMember()] public string Brain;

        #endregion

    }
}
