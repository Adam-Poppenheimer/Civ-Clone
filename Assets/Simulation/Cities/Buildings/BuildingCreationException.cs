using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Cities.Buildings {

    [Serializable]
    public class BuildingCreationException : Exception {

        #region constructors

        public BuildingCreationException() { }

        public BuildingCreationException(string message) : base(message) { }

        public BuildingCreationException(string message, Exception innerException) : base(message, innerException) { }

        protected BuildingCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}