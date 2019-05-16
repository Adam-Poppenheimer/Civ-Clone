using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// An exception that is thrown whenever the creation of a city goes awry.
    /// </summary>
    [Serializable]
    public class CityCreationException : Exception {

        #region constructors

        /// <inheritdoc/>
        public CityCreationException() { }

        /// <inheritdoc/>
        public CityCreationException(string message) : base(message) { }

        /// <inheritdoc/>
        public CityCreationException(string message, Exception innerException) : base(message, innerException) { }

        /// <inheritdoc/>
        protected CityCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}