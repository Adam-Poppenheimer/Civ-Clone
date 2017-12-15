using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.UI.Units {

    public class UnitAbilitiesDisplay : UnitDisplayBase {

        #region instance fields and properties

        private AbilityDisplayMemoryPool AbilityDisplayPool;

        private List<IAbilityDisplay> ActiveAbilityDisplays = new List<IAbilityDisplay>();

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(AbilityDisplayMemoryPool abilityDisplayPool) {
            AbilityDisplayPool = abilityDisplayPool;
        }

        #region from UnitDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var display in ActiveAbilityDisplays) {
                AbilityDisplayPool.Despawn(display);
            }
            ActiveAbilityDisplays.Clear();

            foreach(var ability in ObjectToDisplay.Template.Abilities) {
                var newDisplay = AbilityDisplayPool.Spawn();

                newDisplay.transform.SetParent(transform, false);
                newDisplay.UnitToInvokeOn = ObjectToDisplay;
                newDisplay.AbilityToDisplay = ability;
                newDisplay.Refresh();

                ActiveAbilityDisplays.Add(newDisplay);
            }
        }

        #endregion

        #endregion

    }

}
