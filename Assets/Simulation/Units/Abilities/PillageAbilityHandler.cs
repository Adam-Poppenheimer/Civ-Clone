using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Improvements;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class PillageAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private IImprovementLocationCanon                     ImprovementLocationCanon;
        private ICivilizationTerritoryLogic                   CivTerritoryLogic;
        private IWarCanon                                     WarCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public PillageAbilityHandler(
            IUnitPositionCanon unitPositionCanon, IImprovementLocationCanon improvementLocationCanon,
            ICivilizationTerritoryLogic civTerritoryLogic, IWarCanon warCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            UnitPositionCanon        = unitPositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            CivTerritoryLogic        = civTerritoryLogic;
            WarCanon                 = warCanon;
            UnitPossessionCanon      = unitPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(command.Type != AbilityCommandType.Pillage) {
                return false;
            }

            var unitLocation = UnitPositionCanon  .GetOwnerOfPossession(unit);
            var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);

            bool hasImprovement = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).Any(
                improvement => !improvement.IsPillaged
            );

            if(hasImprovement || unitLocation.HasRoads) {
                var cellOwner = CivTerritoryLogic.GetCivClaimingCell(unitLocation);

                return cellOwner == null || WarCanon.AreAtWar(unitOwner, cellOwner);
            }else {
                return false;
            }
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var firstImprovement = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                if(firstImprovement != null) {
                    firstImprovement.Pillage();
                }else {
                    unitLocation.HasRoads = false;
                }

                unit.CurrentMovement--;
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
