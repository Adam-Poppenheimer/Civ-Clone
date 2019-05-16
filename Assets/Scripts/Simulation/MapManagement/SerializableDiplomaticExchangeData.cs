using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    [Serializable, DataContract]
    public class SerializableDiplomaticExchangeData {

        [DataMember()] public ExchangeType    Type;
        [DataMember()] public int             IntegerInput;
        [DataMember()] public HexCoordinates? CityInputLocation;
        [DataMember()] public string          ResourceInput;
    
    }

}
