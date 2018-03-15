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
        public ReadOnlyCollection<IWorkerSlot> Slots {
            get { return _slots.AsReadOnly(); }
        }
        private List<IWorkerSlot> _slots;

        public string name {
            get { return Template.name; }
        }

        public int Maintenance {
            get { return Template.Maintenance; }
        }

        public ResourceSummary StaticYield {
            get { return Template.StaticYield; }
        }

        public ResourceSummary CivilizationYieldModifier {
            get { return Template.CivilizationYieldModifier; }
        }

        public ResourceSummary CityYieldModifier {
            get { return Template.CityYieldModifier; }
        }

        public IBuildingTemplate Template { get; private set; }

        #endregion

        #endregion

        #region constructors

        /// <summary>
        /// Constructs a Building object from the given IBuildingTemplate.
        /// </summary>
        /// <param name="template">The template that'll define most of the building's properties</param>
        public Building(IBuildingTemplate template, List<IWorkerSlot> slots) {
            Template = template;
            _slots   = slots;
        }

        #endregion

    }

}
