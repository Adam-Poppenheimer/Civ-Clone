using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.WorkerSlots {

    public class WorkerSlotFactory : IWorkerSlotFactory {

        #region instance fields and properties

        private DiContainer Container;

        #endregion

        #region constructors

        [Inject]
        public WorkerSlotFactory(DiContainer container) {
            Container = container;
        }

        #endregion

        #region instance methods

        #region from IWorkerSlotFactory

        public IWorkerSlot BuildSlot(IHexCell parentCell) {
            var newSlot = Container.Instantiate<WorkerSlot>();

            newSlot.ParentCell = parentCell;

            return newSlot;
        }

        public IWorkerSlot BuildSlot(IBuilding parentBuilding) {
            var newSlot = Container.Instantiate<WorkerSlot>();

            newSlot.ParentBuilding = parentBuilding;

            return newSlot;
        }

        #endregion

        #endregion

    }

}
