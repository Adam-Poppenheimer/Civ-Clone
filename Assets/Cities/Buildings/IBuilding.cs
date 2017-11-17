using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.GameMap;
using Assets.Cities;

namespace Assets.Cities.Buildings {

    public interface IBuilding {

        #region properties

        IBuildingTemplate Template { get; }

        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        #endregion

    }

}
