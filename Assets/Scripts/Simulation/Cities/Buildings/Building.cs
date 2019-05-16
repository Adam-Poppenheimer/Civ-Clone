using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuilding.
    /// </summary>
    public class Building : IBuilding {

        #region instance fields and properties

        #region from IBuilding

        /// <inheritdoc/>
        public ReadOnlyCollection<IWorkerSlot> Slots { get; set; }

        public string name {
            get { return Template.name; }
        }

        public int Maintenance {
            get { return Template.Maintenance; }
        }

        public YieldSummary StaticYield {
            get { return Template.StaticYield; }
        }

        public YieldSummary CivilizationYieldModifier {
            get { return Template.CivilizationYieldModifier; }
        }

        public YieldSummary CityYieldModifier {
            get { return Template.CityYieldModifier; }
        }

        public IBuildingTemplate Template { get; private set; }

        #endregion

        #endregion

        #region constructors

        public Building(IBuildingTemplate template) {
            Template = template;
        }

        #endregion

    }

}
