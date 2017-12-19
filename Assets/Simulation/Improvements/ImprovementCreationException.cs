using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Improvements {

    [Serializable]
    public class ImprovementCreationException : Exception {

        #region constructors

        public ImprovementCreationException() { }

        public ImprovementCreationException(string message) : base(message) { }

        public ImprovementCreationException(string message, Exception innerException) : base(message, innerException) { }

        protected ImprovementCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}