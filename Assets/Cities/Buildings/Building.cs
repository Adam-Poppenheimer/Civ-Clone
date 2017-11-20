using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Assets.Cities.Buildings {

    public class Building : IBuilding {

        #region instance fields and properties

        #region from IBuilding

        public ReadOnlyCollection<IWorkerSlot> Slots {
            get { return _slots.AsReadOnly(); }
        }
        private List<IWorkerSlot> _slots;

        public IBuildingTemplate Template { get; private set; }

        #endregion

        #endregion

        #region constructors

        public Building(IBuildingTemplate template) {
            Template = template;
            _slots = template.SlotYields.Select(yield => new WorkerSlot(yield) { IsOccupiable = true } as IWorkerSlot).ToList();
        }

        #endregion

    }

}
