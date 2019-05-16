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

        #region instance fields and properties

        private List<IBarbarianGoalBrain> GoalBrains;

        #endregion

        #region constructors

        [Inject]
        public BarbarianUnitBrain(List<IBarbarianGoalBrain> goalBrains) {
            GoalBrains = goalBrains;
        }

        #endregion

        #region instance methods

        #region from IBarbarianUnitBrain

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            IBarbarianGoalBrain brainToExecute = GoalBrains.MaxElement(brain => brain.GetUtilityForUnit(unit, maps));

            if(brainToExecute != null) {
                return brainToExecute.GetCommandsForUnit(unit, maps);
            }else {
                return new List<IUnitCommand>();
            }
        }

        #endregion      

        #endregion

    }

}
