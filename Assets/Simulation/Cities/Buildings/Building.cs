using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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

        /// <inheritdoc/>
        public IBuildingTemplate Template { get; private set; }

        #endregion

        #endregion

        #region constructors

        /// <summary>
        /// Constructs a Building object from the given IBuildingTemplate.
        /// </summary>
        /// <param name="template">The template that'll define most of the building's properties</param>
        public Building(IBuildingTemplate template) {
            Template = template;
            _slots = template.SlotYields.Select(yield => new WorkerSlot(yield) as IWorkerSlot).ToList();
        }

        #endregion

    }

}
