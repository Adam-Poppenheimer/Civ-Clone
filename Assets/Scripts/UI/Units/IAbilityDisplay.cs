using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.UI.Units {

    public interface IAbilityDisplay {

        #region properties

        IAbilityDefinition AbilityToDisplay { get; set; }

        IUnit UnitToInvokeOn { get; set; }

        Transform transform { get; }

        Button ExecuteButton { get; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

    public class AbilityDisplayMemoryPool : MemoryPool<IAbilityDisplay> {

        #region instance methods

        #region from MemoryPool<IAbilityDisplay>

        protected override void OnCreated(IAbilityDisplay item) {
            item.transform.gameObject.SetActive(false);
        }

        protected override void OnSpawned(IAbilityDisplay item) {
            item.transform.gameObject.SetActive(true);
        }

        protected override void OnDespawned(IAbilityDisplay item) {
            item.transform.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
