using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Cities {

    [Serializable]
    public class CityCreationException : Exception {

        #region constructors

        public CityCreationException() { }

        public CityCreationException(string message) : base(message) { }

        public CityCreationException(string message, Exception innerException) : base(message, innerException) { }

        protected CityCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}