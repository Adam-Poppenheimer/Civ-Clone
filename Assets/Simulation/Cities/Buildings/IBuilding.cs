using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all buildings, which are objects constructed within
    /// cities that have various effects.
    /// </summary>
    public interface IBuilding {

        #region properties

        string name { get; }

        int Maintenance { get; }

        ResourceSummary StaticYield { get; }

        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        ResourceSummary CivilizationYieldModifier { get; }

        ResourceSummary CityYieldModifier { get; }

        int Health { get; }

        int Happiness { get; }

        IBuildingTemplate Template { get; }

        #endregion

    }

}
