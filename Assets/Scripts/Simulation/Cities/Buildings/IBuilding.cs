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

        YieldSummary StaticYield { get; }

        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        YieldSummary CivilizationYieldModifier { get; }

        YieldSummary CityYieldModifier { get; }

        IBuildingTemplate Template { get; }

        #endregion

    }

}
