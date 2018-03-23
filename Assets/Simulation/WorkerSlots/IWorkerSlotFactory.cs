using System;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;


namespace Assets.Simulation.WorkerSlots {

    public interface IWorkerSlotFactory {

        #region methods

        IWorkerSlot BuildSlot(IHexCell parentCell);

        IWorkerSlot BuildSlot(IBuilding parentBuilding);

        #endregion

    }

}