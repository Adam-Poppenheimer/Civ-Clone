using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public class BarbarianUnitBrain : IBarbarianUnitBrain {

        #region internal types

        private enum GoalType {
            Wander = 0,
        }

        #endregion

        #region instance fields and properties

        private IBarbarianWanderBrain WanderBrain;

        #endregion

        #region constructors

        [Inject]
        public BarbarianUnitBrain(IBarbarianWanderBrain wanderBrain) {
            WanderBrain = wanderBrain;
        }

        #endregion

        #region instance methods

        #region from IBarbarianUnitBrain

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            switch(GetGoalForUnit(unit, maps)) {
                case GoalType.Wander: return WanderBrain.GetWanderCommandsForUnit(unit, maps);
                default: throw new NotImplementedException();
            }
        }

        #endregion

        private GoalType GetGoalForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            return GoalType.Wander;
        }        

        #endregion

    }

}
