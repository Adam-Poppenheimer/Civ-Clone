using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationTerritoryLogic {

        #region methods

        ICivilization GetCivClaimingCell(IHexCell cell);

        IEnumerable<IHexCell> GetCellsClaimedByCiv(ICivilization civ);

        #endregion

    }

}
