using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// An exception that throws whenever a city reports a negative number of
    /// unemployed citizens.
    /// </summary>
    public class NegativeUnemploymentException : Exception {

        #region constructors
        
        /// <inheritdoc/>
        public NegativeUnemploymentException() { }

        /// <inheritdoc/>
        public NegativeUnemploymentException(string message) : base(message) { }

        /// <inheritdoc/>
        public NegativeUnemploymentException(string message, Exception inner) : base(message, inner) { }

        #endregion

    }

}
