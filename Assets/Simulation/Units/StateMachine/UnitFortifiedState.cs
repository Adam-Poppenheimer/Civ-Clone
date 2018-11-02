using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.StateMachine {

    public class UnitFortifiedState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit UnitToControl;

        private IUnitFortificationLogic FortificationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Unit To Control")] IUnit unitToControl,
            IUnitFortificationLogic fortificationLogic
        ) {
            UnitToControl      = unitToControl;
            FortificationLogic = fortificationLogic;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            FortificationLogic.SetFortificationStatusForUnit(UnitToControl, true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            FortificationLogic.SetFortificationStatusForUnit(UnitToControl, false);
        }

        #endregion

        #endregion

    }

}
