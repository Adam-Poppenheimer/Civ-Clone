using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;

namespace Assets.Simulation.Improvements {

    public class ImprovementConstructionExecuter : IImprovementConstructionExecuter {

        #region instance fields and properties

        private IUnitPositionCanon        UnitPositionCanon;
        private IImprovementLocationCanon ImprovementLocationCanon;
        private IImprovementWorkLogic     ImprovementWorkLogic;

        #endregion

        #region constructors

        [Inject]
        public ImprovementConstructionExecuter(
            IUnitPositionCanon unitPositionCanon, IImprovementLocationCanon improvementLocationCanon,
            IImprovementWorkLogic improvementWorkLogic
        ) {
            UnitPositionCanon        = unitPositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            ImprovementWorkLogic     = improvementWorkLogic;
        }

        #endregion

        #region instance methods

        #region from IImprovementConstructionExecuter

        public void PerformImprovementConstruction(IUnit unit) {
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

        #endregion

    }

}
