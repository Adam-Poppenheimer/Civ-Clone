using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.GameMap;

namespace Assets.Cities {

    public interface ITilePossessionCanon {

        #region methods

        ReadOnlyCollection<IMapTile> GetTilesOfCity(ICity city);

        ICity GetCityOfTile(IMapTile tile);

        bool CanChangeOwnerOfTile(IMapTile tile, ICity newOwner);
        void ChangeOwnerOfTile(IMapTile tile, ICity newOwner);

        #endregion

    }

}
