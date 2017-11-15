using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameMap;

namespace Assets.Cities {

    public class TileExpansionLogic : ITileExpansionLogic {

        #region instance methods

        #region from ICulturalExpansionLogic

        public bool TileIsAvailable(ICity city, IMapTile tile) {
            throw new NotImplementedException();
        }

        public int GetCultureCostOfAcquiringTile(ICity city, IMapTile tile) {
            throw new NotImplementedException();
        }

        public int GetGoldCostOfAcquiringTile(ICity city, IMapTile tile) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
