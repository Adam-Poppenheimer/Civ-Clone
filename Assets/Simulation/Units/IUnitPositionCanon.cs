using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitPositionCanon : IPossessionRelationship<IHexCell, IUnit> {

        #region methods

        bool CanPlaceUnitOfTypeAtLocation(UnitType type, IHexCell location);

        #endregion

    }

}
