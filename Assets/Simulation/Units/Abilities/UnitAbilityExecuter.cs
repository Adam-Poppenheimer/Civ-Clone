using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units.Abilities {

    public class UnitAbilityExecuter : IUnitAbilityExecuter {

        #region instance fields and properties

        private UnitSignals Signals;

        private IEnumerable<IUnitAbilityHandler> AbilityHandlers;

        public IEnumerable<IOngoingAbility> OngoingAbilities {
            get { return ongoingAbilities; }
        }
        private List<IOngoingAbility> ongoingAbilities = new List<IOngoingAbility>();

        #endregion

        #region constructors

        [Inject]
        public UnitAbilityExecuter(UnitSignals signals,
            [Inject(Id = "Unit Ability Handlers")] IEnumerable<IUnitAbilityHandler> abilityHandlers
        ){
            Signals         = signals;
            AbilityHandlers = abilityHandlers;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityExecuter

        public bool CanExecuteAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            foreach(var handler in AbilityHandlers) {
                if(handler.CanHandleAbilityOnUnit(ability, unit)) {
                    return true;
                }
            }

            return false;
        }

        public void ExecuteAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            foreach(var handler in AbilityHandlers) {
                var results = handler.TryHandleAbilityOnUnit(ability, unit);
                if(results.AbilityHandled) {
                    if(results.NewAbilityActivated != null) {
                        results.NewAbilityActivated.BeginExecution();
                        ongoingAbilities.Add(results.NewAbilityActivated);
                    }
                    Signals.UnitActivatedAbilitySignal.OnNext(new UniRx.Tuple<IUnit, IUnitAbilityDefinition>(unit, ability));
                    return;
                }
            }

            throw new AbilityNotHandledException("No handler was able to handle the ability");
        }

        public void PerformOngoingAbilities() {
            foreach(var ability in new List<IOngoingAbility>(ongoingAbilities)) {
                ability.TickExecution();

                if(ability.IsReadyToTerminate()) {
                    ability.TerminateExecution();
                    ongoingAbilities.Remove(ability);
                }
            }
        }

        #endregion

        #endregion
        
    }

}
