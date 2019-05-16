using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Improvements;
using Assets.Simulation.Units;

namespace Assets.Simulation.Core {

    public class UnitRoundExecuter : IRoundExecuter {

        #region instance fields and properties

        private IUnitFactory                     UnitFactory;
        private IUnitHealingLogic                UnitHealingLogic;
        private IImprovementDamageExecuter       ImprovementDamageExecuter;
        private IImprovementConstructionExecuter ImprovementConstructionExecuter;


        #endregion

        #region constructors

        [Inject]
        public UnitRoundExecuter(
            IUnitFactory unitFactory, IUnitHealingLogic unitHealingLogic, IImprovementDamageExecuter improvementDamageExecuter,
            IImprovementConstructionExecuter improvementConstructionExecuter
        ) {
            UnitFactory                     = unitFactory;
            UnitHealingLogic                = unitHealingLogic;
            ImprovementDamageExecuter       = improvementDamageExecuter;
            ImprovementConstructionExecuter = improvementConstructionExecuter;
        }

        #endregion

        #region instance methods

        #region from IRoundExecuter

        public void PerformStartOfRoundActions() {
            foreach(var unit in UnitFactory.AllUnits) {
                UnitHealingLogic.PerformHealingOnUnit(unit);
                unit.CurrentMovement = unit.MaxMovement;
            }
        }

        public void PerformEndOfRoundActions() {
            foreach(var unit in UnitFactory.AllUnits) {
                unit.PerformMovement();
                unit.CanAttack = true;

                ImprovementConstructionExecuter.PerformImprovementConstruction                (unit);
                ImprovementDamageExecuter      .PerformDamageOnUnitFromImprovements(unit);
            }
        }

        #endregion

        #endregion

    }

}
