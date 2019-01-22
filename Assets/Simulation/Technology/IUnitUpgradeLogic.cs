using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Technology {

    public interface IUnitUpgradeLogic {

        #region properties

        IEnumerable<IUnitUpgradeLine> AllUpgradeLines { get; }

        #endregion

        #region methods

        IUnitUpgradeLine GetUpgradeLineForUnit(IUnit unit);

        IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCiv(ICivilization civ);
        
        IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCivs(params ICivilization[] civs);
        IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCivs(IEnumerable<ICivilization> civs);

        #endregion

    }

}
