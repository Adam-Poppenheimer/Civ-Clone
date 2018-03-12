﻿using System;
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

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            var roadBuildingCommands = ability.CommandRequests.Where(command => command.CommandType == AbilityCommandType.BuildRoad);

            if(roadBuildingCommands.Count() > 0) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return unitLocation != null && !unitLocation.HasRoads;
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                unitLocation.HasRoads = true;

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
