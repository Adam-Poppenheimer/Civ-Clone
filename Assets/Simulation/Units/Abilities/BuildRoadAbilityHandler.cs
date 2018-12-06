using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.Abilities {

    public class BuildRoadAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildRoadAbilityHandler(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return command.Type == AbilityCommandType.BuildRoad
                && unitLocation != null
                && !unitLocation.HasRoads;
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                unitLocation.HasRoads = true;
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
