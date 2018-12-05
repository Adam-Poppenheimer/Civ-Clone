using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class RepairAdjacentShipsAbilityHandler : IAbilityHandler {

        #region instance fields and properties
        
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IHexGrid                                      Grid;

        #endregion

        #region constructors

        [Inject]
        public RepairAdjacentShipsAbilityHandler(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IHexGrid grid
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            UnitPositionCanon   = unitPositionCanon;
            Grid                = grid;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            return ability.CommandRequests.Any(command => command.CommandType == AbilityCommandType.RepairAdjacentShips);
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);
                var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);

                var nearbyCells = Grid.GetCellsInRadius(unitPosition, 1);

                foreach(var nearbyUnit in nearbyCells.SelectMany(cell => UnitPositionCanon.GetPossessionsOfOwner(cell))) {
                    var nearbyOwner = UnitPossessionCanon.GetOwnerOfPossession(nearbyUnit);

                    if(nearbyUnit.Type.IsWaterMilitary() && nearbyOwner == unitOwner) {
                        nearbyUnit.CurrentHitpoints = nearbyUnit.MaxHitpoints;
                    }
                }

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
