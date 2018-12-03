using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Core {

    /// <summary>
    /// The standard implementation of ITurnExecuter.
    /// </summary>
    public class RoundExecuter : IRoundExecuter {

        #region instance fields and properties

        private IUnitPositionCanon        UnitPositionCanon;
        private IImprovementLocationCanon ImprovementLocationCanon;
        private IUnitHealingLogic         UnitHealingLogic;
        private IImprovementWorkLogic     ImprovementWorkLogic;

        #endregion

        #region constructors

        public RoundExecuter(
            IUnitPositionCanon unitPositionCanon, IImprovementLocationCanon improvementLocationCanon,
            IUnitHealingLogic unitHealingLogic, IImprovementWorkLogic improvementWorkLogic
        ){
            UnitPositionCanon        = unitPositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            UnitHealingLogic         = unitHealingLogic;
            ImprovementWorkLogic     = improvementWorkLogic;
        }

        #endregion

        #region instance methods

        #region from ITurnExecuter

        /// <inheritdoc/>
        public void BeginRoundOnCity(ICity city) {
            city.PerformProduction();
            city.PerformGrowth();
            city.PerformExpansion();
            city.PerformDistribution();
        }

        /// <inheritdoc/>
        public void EndRoundOnCity(ICity city) {
            city.PerformIncome();
        }

        /// <inheritdoc/>
        public void BeginRoundOnCivilization(ICivilization civilization) {
            civilization.PerformIncome();
            civilization.PerformResearch();
            civilization.PerformGreatPeopleGeneration();
            civilization.PerformGoldenAgeTasks();
        }

        /// <inheritdoc/>
        public void EndRoundOnCivilization(ICivilization civilization) {
            
        }

        /// <inheritdoc/>
        public void BeginRoundOnUnit(IUnit unit) {
            UnitHealingLogic.PerformHealingOnUnit(unit);
            unit.CurrentMovement = unit.MaxMovement;            
        }

        /// <inheritdoc/>
        public void EndRoundOnUnit(IUnit unit) {
            unit.PerformMovement();
            PerformConstruction(unit);
            unit.CanAttack = true;            
        }

        #endregion

        private void PerformConstruction(IUnit unit) {
            if(unit.LockedIntoConstruction && unit.CurrentMovement > 0) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);
                
                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                if(improvementAtLocation != null && !improvementAtLocation.IsConstructed) {
                    improvementAtLocation.WorkInvested += ImprovementWorkLogic.GetWorkOfUnitOnImprovement(unit, improvementAtLocation);
                    unit.CurrentMovement = 0f;

                    if(improvementAtLocation.IsReadyToConstruct) {
                        improvementAtLocation.Construct();
                    }
                }
            }
        }

        #endregion

    }

}
