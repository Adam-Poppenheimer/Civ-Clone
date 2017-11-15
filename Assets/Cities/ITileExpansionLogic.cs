using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.GameMap;

namespace Assets.Cities {

    public interface ITileExpansionLogic {

        #region methods

        bool TileIsAvailable(ICity city, IMapTile tile);

        int GetCultureCostOfAcquiringTile(ICity city, IMapTile tile);
        int GetGoldCostOfAcquiringTile(ICity city, IMapTile tile);

        #endregion

    }

}
