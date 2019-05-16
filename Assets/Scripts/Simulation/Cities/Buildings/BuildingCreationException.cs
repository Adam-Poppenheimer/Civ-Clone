using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// An exception that is thrown whenever building creation goes awry.
    /// </summary>
    [Serializable]
    public class BuildingCreationException : Exception {

        #region constructors

        /// <inheritdoc/>
        public BuildingCreationException() { }

        /// <inheritdoc/>
        public BuildingCreationException(string message) : base(message) { }

        /// <inheritdoc/>
        public BuildingCreationException(string message, Exception innerException) : base(message, innerException) { }

        /// <inheritdoc/>
        protected BuildingCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

    }

}