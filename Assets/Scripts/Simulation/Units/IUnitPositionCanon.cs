using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public interface IUnitPositionCanon : IPossessionRelationship<IHexCell, IUnit> {

        #region methods

        bool CanPlaceUnitAtLocation(IUnit unit, IHexCell location, bool isMeleeAttacking);

        bool CanPlaceUnitTemplateAtLocation(IUnitTemplate template, IHexCell location, ICivilization owner);

        float GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell, bool isMeleeAttacking);

        Func<IHexCell, IHexCell, float> GetPathfindingCostFunction(IUnit unit, bool isMeleeAttacking);

        #endregion

    }

}
