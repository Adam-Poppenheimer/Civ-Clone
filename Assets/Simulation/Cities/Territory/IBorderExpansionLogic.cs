using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Territory {

    public interface IBorderExpansionLogic {

        #region methods

        IEnumerable<IHexCell> GetAllTilesAvailableToCity(ICity city);

        IHexCell GetNextTileToPursue(ICity city);

        bool IsTileAvailable(ICity city, IHexCell tile);

        int GetCultureCostOfAcquiringTile(ICity city, IHexCell tile);
        int GetGoldCostOfAcquiringTile(ICity city, IHexCell tile);

        #endregion

    }

}
