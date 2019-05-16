using System;
using System.Runtime.Serialization;

namespace Assets.Simulation.Units.Abilities {

    [Serializable]
    public class AbilityNotHandledException : Exception {

        #region constructors

        public AbilityNotHandledException() {
        }

        public AbilityNotHandledException(string message) : base(message) {
        }

        public AbilityNotHandledException(string message, Exception innerException) : base(message, innerException) {
        }

        protected AbilityNotHandledException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        #endregion

    }

}