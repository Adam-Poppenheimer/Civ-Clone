using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface IStartingUnitPlacementLogic {

        #region methods

        void PlaceStartingUnitsInRegion(MapRegion region, ICivilization owner, IMapTemplate template);

        #endregion

    }

}
