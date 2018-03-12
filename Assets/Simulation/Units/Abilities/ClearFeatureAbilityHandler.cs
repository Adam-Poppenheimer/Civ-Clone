using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units.Abilities {

    public class ClearFeatureAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public ClearFeatureAbilityHandler(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            var firstCommand = ability.CommandRequests.FirstOrDefault();

            if(ability.CommandRequests.Count() == 1 && firstCommand.CommandType == AbilityCommandType.ClearFeature) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return unitLocation.Feature.ToString().Equals(firstCommand.ArgsToPass.FirstOrDefault());
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                unitLocation.Feature = TerrainFeature.None;

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
