﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units.Abilities {

    public class UnitAbilityExecuter : IUnitAbilityExecuter {

        #region instance fields and properties

        private IEnumerable<IUnitAbilityHandler> AbilityHandlers;

        #endregion

        #region constructors

        [Inject]
        public UnitAbilityExecuter([Inject(Id = "Unit Ability Handlers")] IEnumerable<IUnitAbilityHandler> abilityHandlers) {
            AbilityHandlers = abilityHandlers;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityExecuter

        public void ExecuteAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            foreach(var handler in AbilityHandlers) {
                if(handler.TryHandleAbilityOnUnit(ability, unit)) {
                    return;
                }
            }

            throw new AbilityNotHandledException("No handler was able to handle the ability");
        }

        #endregion

        #endregion
        
    }

}
