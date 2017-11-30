using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public class NegativeUnemploymentException : Exception {

        #region constructors

        public NegativeUnemploymentException() { }
        public NegativeUnemploymentException(string message) : base(message) { }
        public NegativeUnemploymentException(string message, Exception inner) : base(message, inner) { }

        #endregion

    }

}
