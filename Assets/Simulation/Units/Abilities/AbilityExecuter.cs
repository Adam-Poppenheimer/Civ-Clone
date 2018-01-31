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
        public AbilityExecuter(UnitSignals signals,
            [Inject(Id = "Unit Ability Handlers")] IEnumerable<IAbilityHandler> abilityHandlers
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

            foreach(var handler in AbilityHandlers) {
                if(handler.CanHandleAbilityOnUnit(ability, unit)) {
                    return true;
                }
            }

            return false;
        }

        public void ExecuteAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            foreach(var handler in AbilityHandlers) {
                if(!CanExecuteAbilityOnUnit(ability, unit)) {
                    throw new InvalidOperationException("CanExecuterAbilityOnUnit must return true on the arguments");
                }

                var results = handler.TryHandleAbilityOnUnit(ability, unit);
                if(results.AbilityHandled) {

                    if(ability.ConsumesMovement) {
                        unit.CurrentMovement = 0;
                    }

                    if(ability.DestroysUnit) {
                        if(Application.isPlaying) {
                            GameObject.Destroy(unit.gameObject);
                        }else {
                            GameObject.DestroyImmediate(unit.gameObject);
                        }  
                    }

                    if(results.NewAbilityActivated != null) {
                        results.NewAbilityActivated.BeginExecution();
                        ongoingAbilities.Add(results.NewAbilityActivated);
                    }
                    Signals.ActivatedAbilitySignal.OnNext(new UniRx.Tuple<IUnit, IAbilityDefinition>(unit, ability));
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
