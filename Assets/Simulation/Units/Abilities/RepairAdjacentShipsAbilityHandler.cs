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

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            return command.Type == AbilityCommandType.RepairAdjacentShips;
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);
                var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);

                var nearbyCells = Grid.GetCellsInRadius(unitPosition, 1);

                foreach(var nearbyUnit in nearbyCells.SelectMany(cell => UnitPositionCanon.GetPossessionsOfOwner(cell))) {
                    var nearbyOwner = UnitPossessionCanon.GetOwnerOfPossession(nearbyUnit);

                    if(nearbyUnit.Type.IsWaterMilitary() && nearbyOwner == unitOwner) {
                        nearbyUnit.CurrentHitpoints = nearbyUnit.MaxHitpoints;
                    }
                }
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
