using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Units {

    [Serializable]
    public class UnitCreationException : Exception {

        #region constructors

        public UnitCreationException() { }

        public UnitCreationException(string message) : base(message) { }

        public UnitCreationException(string message, Exception innerException) : base(message, innerException) { }

        protected UnitCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}