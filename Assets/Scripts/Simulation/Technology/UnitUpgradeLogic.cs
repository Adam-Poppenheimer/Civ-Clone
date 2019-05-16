using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Technology {

    public class UnitUpgradeLogic : IUnitUpgradeLogic {

        #region instance fields and properties

        #region from IUnitUpgradeLogic

        public IEnumerable<IUnitUpgradeLine> AllUpgradeLines { get; private set; }

        #endregion

        private ITechCanon TechCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitUpgradeLogic(
            [Inject(Id = "All Upgrade Lines")] IEnumerable<IUnitUpgradeLine> allUpgradeLines,
            ITechCanon techCanon
        ) {
            AllUpgradeLines = allUpgradeLines;
            TechCanon       = techCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitUpgradeLogic

        public IUnitUpgradeLine GetUpgradeLineForUnit(IUnit unit) {
            return AllUpgradeLines.FirstOrDefault(line => line.Units.Contains(unit.Template));
        }

        public IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCiv(ICivilization civ) {
            return AllUpgradeLines.Select(
                line => line.Units.LastOrDefault(unitTemplate => TechCanon.IsUnitResearchedForCiv(unitTemplate, civ))

            ).Where(line => line != null).Distinct();
        }

        public IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCivs(params ICivilization[] civs) {
            return GetCuttingEdgeUnitsForCivs(civs as IEnumerable<ICivilization>);
        }

        public IEnumerable<IUnitTemplate> GetCuttingEdgeUnitsForCivs(IEnumerable<ICivilization> civs) {
            return AllUpgradeLines.Select(
                line => line.Units.LastOrDefault(unitTemplate => civs.Any(civ => TechCanon.IsUnitResearchedForCiv(unitTemplate, civ)))

            ).Where(line => line != null).Distinct();
        }

        #endregion

        #endregion

    }

}
