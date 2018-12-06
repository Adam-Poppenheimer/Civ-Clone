using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.Abilities {

    public class AbilityExecuter : IAbilityExecuter {

        #region instance fields and properties

        private UnitSignals Signals;

        private IEnumerable<IAbilityHandler> AbilityHandlers;

        public IEnumerable<IOngoingAbility> OngoingAbilities {
            get { return ongoingAbilities; }
        }
        private List<IOngoingAbility> ongoingAbilities = new List<IOngoingAbility>();

        #endregion

        #region constructors

        [Inject]
        public AbilityExecuter(
            UnitSignals signals, List<IAbilityHandler> abilityHandlers
        ){
            Signals         = signals;
            AbilityHandlers = abilityHandlers;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityExecuter

        public bool CanExecuteAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(ability.RequiresMovement && unit.CurrentMovement <= 0) {
                return false;
            }

            foreach(var command in ability.CommandRequests) {
                if(!AbilityHandlers.Any(handler => handler.CanHandleCommandOnUnit(command, unit))) {
                    return false;
                }
            }

            return true;
        }

        public void ExecuteAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(!CanExecuteAbilityOnUnit(ability, unit)) {
                throw new InvalidOperationException("CanExecuterAbilityOnUnit must return true on the arguments");
            }

            foreach(var command in ability.CommandRequests) {
                var firstValidHandler = AbilityHandlers.First(
                    handler => handler.CanHandleCommandOnUnit(command, unit)
                );

                firstValidHandler.HandleCommandOnUnit(command, unit);
            }

            if(ability.ConsumesMovement) {
                unit.CurrentMovement = 0;
            }

            if(ability.DestroysUnit) {
                unit.Destroy(); 
            }

            Signals.ActivatedAbilitySignal.OnNext(new UniRx.Tuple<IUnit, IAbilityDefinition>(unit, ability));
        }

        #endregion

        #endregion
        
    }

}
