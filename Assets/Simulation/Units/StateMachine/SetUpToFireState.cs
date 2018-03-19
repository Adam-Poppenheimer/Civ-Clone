using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.StateMachine {

    public class SetUpToFireState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit UnitToControl;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies([Inject(Id = "Unit To Control")]IUnit unitToControl) {
            UnitToControl = unitToControl;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            
        }

        #endregion

        #endregion

    }

}
