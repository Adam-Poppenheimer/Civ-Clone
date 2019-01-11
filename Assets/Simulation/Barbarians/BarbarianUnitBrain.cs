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

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            IBarbarianGoalBrain brainToExecute = null;
            float lastHighestUtility = 0f;

            foreach(var thisBrain in GoalBrains) {
                if(brainToExecute == null) {
                    brainToExecute = thisBrain;

                }else {
                    float thisUtility = thisBrain.GetUtilityForUnit(unit, maps);

                    if(thisUtility > lastHighestUtility) {
                        brainToExecute = thisBrain;

                        lastHighestUtility = thisUtility;
                    }
                }
            }

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
