using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units.Abilities {

    public class ClearVegetationAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IUnitPositionCanon     UnitPositionCanon;
        private ICellModificationLogic CellModificationLogic;

        #endregion

        #region constructors

        [Inject]
        public ClearVegetationAbilityHandler(
            IUnitPositionCanon unitPositionCanon, ICellModificationLogic cellModificationLogic
        ) {
            UnitPositionCanon     = unitPositionCanon;
            CellModificationLogic = cellModificationLogic;
        }

        #endregion

        #region instance methods

        #region IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(command.Type == AbilityCommandType.ClearVegetation) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return unitLocation.Vegetation.ToString().Equals(command.ArgsToPass.FirstOrDefault());
            }else {
                return false;
            }
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                CellModificationLogic.ChangeVegetationOfCell(unitLocation, CellVegetation.None);
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
