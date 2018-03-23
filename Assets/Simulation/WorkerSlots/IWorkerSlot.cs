using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.WorkerSlots {

    public interface IWorkerSlot {

        #region properties

        bool IsOccupied { get; set; }

        bool IsLocked { get; set; }

        IHexCell ParentCell { get; }

        IBuilding ParentBuilding { get; }

        #endregion

    }

}
