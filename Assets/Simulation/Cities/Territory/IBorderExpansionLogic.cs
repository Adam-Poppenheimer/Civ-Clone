using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Cities.Territory {

    public interface IBorderExpansionLogic {

        #region methods

        IEnumerable<IMapTile> GetAllTilesAvailableToCity(ICity city);

        IMapTile GetNextTileToPursue(ICity city);

        bool IsTileAvailable(ICity city, IMapTile tile);

        int GetCultureCostOfAcquiringTile(ICity city, IMapTile tile);
        int GetGoldCostOfAcquiringTile(ICity city, IMapTile tile);

        #endregion

    }

}
