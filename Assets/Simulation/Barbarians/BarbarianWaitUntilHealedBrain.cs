using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public class BarbarianWaitUntilHealedBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IBarbarianConfig BarbarianConfig;
        private DiContainer      Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianWaitUntilHealedBrain(
            IBarbarianConfig barbarianConfig, DiContainer container
        ) {
            BarbarianConfig = barbarianConfig;
            Container       = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            if(unit.Type.IsCivilian() || unit.Type.IsWaterMilitary()) {
                return 0f;

            }else {
                return BarbarianConfig.WaitUntilHealedMaxUtility * (1f - ((float)unit.CurrentHitpoints / unit.MaxHitpoints));
            }
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            var fortifyCommand = Container.Instantiate<FortifyUnitCommand>();

            fortifyCommand.UnitToFortify = unit;

            return new List<IUnitCommand>() { fortifyCommand };
        }

        #endregion

        #endregion
        
    }

}
