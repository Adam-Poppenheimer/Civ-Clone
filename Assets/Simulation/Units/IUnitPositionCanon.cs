using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitPositionCanon : IPossessionRelationship<IHexCell, IUnit> {

        #region methods

        bool CanPlaceUnitAtLocation(IUnit unit, IHexCell location, bool ignoreOccupancy);

        bool CanPlaceUnitTemplateAtLocation(IUnitTemplate template, IHexCell location, bool isMeleeAttacking);

        #endregion

    }

}
