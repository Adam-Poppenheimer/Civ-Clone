using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.Units.Abilities {

    public class BuildImprovementOngoingAbility : IOngoingAbility {

        #region instance fields and properties

        public IImprovement ImprovementToConstruct { get; private set; }

        public IUnit SourceUnit { get; private set; }

        private IUnitPositionCanon UnitPositionCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        public BuildImprovementOngoingAbility(IImprovement improvementToConstruct, IUnit sourceUnit,
            IUnitPositionCanon unitPositionCanon, IImprovementLocationCanon improvementLocationCanon
        ){
            ImprovementToConstruct   = improvementToConstruct;
            SourceUnit               = sourceUnit;
            UnitPositionCanon        = unitPositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
        } 

        #endregion

        #region instance methods

        #region from IActivatedAbility

        public void BeginExecution() {
            SourceUnit.CurrentMovement = 0;
            ImprovementToConstruct.WorkInvested++;
        }

        public void TickExecution() {
            if(SourceUnit.CurrentMovement > 0 && !IsReadyToTerminate()) {
                SourceUnit.CurrentMovement = 0;
                ImprovementToConstruct.WorkInvested++;
            }
        }

        public bool IsReadyToTerminate() {
            return ImprovementToConstruct.IsComplete
                || UnitPositionCanon.GetOwnerOfPossession(SourceUnit) != ImprovementLocationCanon.GetOwnerOfPossession(ImprovementToConstruct);
        }

        public void TerminateExecution() {
            
        }        

        #endregion

        #endregion

    }

}
