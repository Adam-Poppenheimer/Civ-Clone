using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public interface IUnitPositionCanon : IPossessionRelationship<IMapTile, IUnit> {

        #region methods

        bool CanPlaceUnitOfTypeAtLocation(UnitType type, IMapTile location);

        #endregion

    }

}
