using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Territory {

    public interface ITilePossessionCanon {

        #region methods

        IEnumerable<IHexCell> GetTilesOfCity(ICity city);

        ICity GetCityOfTile(IHexCell tile);

        bool CanChangeOwnerOfTile(IHexCell tile, ICity newOwner);
        void ChangeOwnerOfTile(IHexCell tile, ICity newOwner);

        #endregion

    }

}
