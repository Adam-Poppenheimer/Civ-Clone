using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all buildings, which are objects constructed within
    /// cities that have various effects.
    /// </summary>
    public interface IBuilding {

        #region properties

        /// <summary>
        /// The template this building is based off of, and which defines most
        /// of its properties.
        /// </summary>
        IBuildingTemplate Template { get; }

        /// <summary>
        /// The slots this building provides to its city.
        /// </summary>
        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        #endregion

    }

}
