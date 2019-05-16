using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Technology;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.AI {

    public class UnitStrengthEstimator : IUnitStrengthEstimator {

        #region instance fields and properties

        private IUnitUpgradeLogic                 UnitUpgradeLogic;
        private ICivilizationFactory              CivFactory;
        private IUnitPositionCanon                UnitPositionCanon;
        private IUnitComparativeStrengthEstimator ComparativeStrengthEstimator;

        #endregion

        #region constructors

        [Inject]
        public UnitStrengthEstimator(
            IUnitUpgradeLogic unitUpgradeLogic, ICivilizationFactory civFactory, IUnitPositionCanon unitPositionCanon,
            IUnitComparativeStrengthEstimator comparativeStrengthEstimator
        ) {
            UnitUpgradeLogic             = unitUpgradeLogic;
            CivFactory                   = civFactory;
            UnitPositionCanon            = unitPositionCanon;
            ComparativeStrengthEstimator = comparativeStrengthEstimator;
        }

        #endregion

        #region instance methods

        #region from IUnitStrengthEstimator

        public float EstimateUnitStrength(IUnit unit) {
            if(unit == null) {
                throw new ArgumentNullException("unit");
            }

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var cuttingEdgeTemplates = UnitUpgradeLogic.GetCuttingEdgeUnitsForCivs(CivFactory.AllCivilizations);

            var cuttingEdgeEstimators = cuttingEdgeTemplates.Select(template => new EstimationUnit(template)).ToArray();

            return cuttingEdgeEstimators.Average(
                estimator => ComparativeStrengthEstimator.EstimateComparativeStrength(unit, estimator, unitLocation)
            );
        }

        public float EstimateUnitDefensiveStrength(IUnit unit, IHexCell location) {
            if(unit == null) {
                throw new ArgumentNullException("unit");
            }
            if(location == null) {
                throw new ArgumentNullException("location");
            }

            var cuttingEdgeTemplates = UnitUpgradeLogic.GetCuttingEdgeUnitsForCivs(CivFactory.AllCivilizations);

            var cuttingEdgeEstimators = cuttingEdgeTemplates.Select(template => new EstimationUnit(template));

            return cuttingEdgeEstimators.Average(
                estimator => ComparativeStrengthEstimator.EstimateComparativeDefensiveStrength(unit, estimator, location)
            );
        }

        #endregion

        #endregion
        
    }

}
