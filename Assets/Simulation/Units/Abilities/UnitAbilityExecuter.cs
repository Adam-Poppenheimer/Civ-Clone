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
                if(handler.TryHandleAbilityOnUnit(ability, unit)) {
                    Signals.UnitActivatedAbilitySignal.OnNext(new UniRx.Tuple<IUnit, IUnitAbilityDefinition>(unit, ability));
                    return;
                }
            }

            throw new AbilityNotHandledException("No handler was able to handle the ability");
        }

        #endregion

        #endregion
        
    }

}
