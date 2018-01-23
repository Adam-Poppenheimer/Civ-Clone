using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Distribution {

    /// <summary>
    /// Enum used by cities and several other classes to choose specific resource
    /// types to focus on when distributing citizens to slots or seeking out new
    /// territory to acquire.
    /// </summary>
    public enum ResourceFocusType {
        /// <summary>
        /// Attempts to maximize total yield of the slot or cell.
        /// </summary>
        TotalYield,

        /// <summary>
        /// Attempts to maximize the food yield of the slot or cell.
        /// </summary>
        Food,

        /// <summary>
        /// Attempts to maximize the gold yield of the slot or cell.
        /// </summary>
        Gold,

        /// <summary>
        /// Attempts to maximize the production yield of the slot or cell.
        /// </summary>
        Production,

        /// <summary>
        /// Attempts to maximize the culture yield of the slot or cell.
        /// </summary>
        Culture,

        Science,
    }

}
