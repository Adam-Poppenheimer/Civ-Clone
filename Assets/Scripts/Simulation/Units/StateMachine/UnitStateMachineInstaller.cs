﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.StateMachine {

    public class UnitStateMachineInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameUnit UnitToControl    = null;
        [SerializeField] private Animator UnitStateMachine = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IUnit>().WithId("Unit To Control").FromInstance(UnitToControl);

            if(UnitStateMachine == null) {
                return;
            }

            UnitStateMachine.Rebind();

            foreach(var behaviour in UnitStateMachine.GetBehaviours<StateMachineBehaviour>()) {
                Container.Rebind(behaviour.GetType()).FromInstance(behaviour);

                Container.QueueForInject(behaviour);
            }
        }

        #endregion

        #endregion

    }

}
