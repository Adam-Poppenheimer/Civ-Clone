using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Assets.GameMap;

namespace Assets.Cities {

    public class TilePossessionCanon : ITilePossessionCanon {

        #region instance methods

        #region from ITilePossessionCanon

        public bool CanChangeOwnerOfTile(IMapTile tile, ICity newOwner) {
            throw new NotImplementedException();
        }

        public void ChangeOwnerOfTile(IMapTile tile, ICity newOwner) {
            throw new NotImplementedException();
        }

        public ICity GetCityOfTile(IMapTile tile) {
            throw new NotImplementedException();
        }

        public ReadOnlyCollection<IMapTile> GetTilesOfCity(ICity city) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
