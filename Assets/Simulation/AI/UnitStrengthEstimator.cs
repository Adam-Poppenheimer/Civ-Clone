using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.AI {

    public class UnitStrengthEstimator : IUnitStrengthEstimator {

        #region instance fields and properties



        #endregion

        #region constructors

        [Inject]
        public UnitStrengthEstimator() {

        }

        #endregion

        #region instance methods

        #region from IUnitStrengthEstimator

        public float EstimateUnitStrength(IUnit unit) {
            return unit.Template.CombatStrength;
        }

        public float EstimateUnitDefensiveStrength(IUnit unit, IHexCell location) {
            return unit.Template.CombatStrength;
        }

        #endregion

        #endregion
        
    }

}
