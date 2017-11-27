using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuilding {

        #region properties

        IBuildingTemplate Template { get; }

        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        #endregion

    }

}
