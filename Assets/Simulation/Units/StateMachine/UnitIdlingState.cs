using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.StateMachine {

    public class UnitIdlingState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit UnitToControl;

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Unit To Control")] IUnit unitToControl, UnitSignals unitSignals
        ){
            UnitToControl = unitToControl;
            UnitSignals   = unitSignals;
        }

        #region from StateMachineBehaviour

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitSignals.BecameIdleSignal.OnNext(UnitToControl);
        }

        #endregion

        #endregion

    }

}
