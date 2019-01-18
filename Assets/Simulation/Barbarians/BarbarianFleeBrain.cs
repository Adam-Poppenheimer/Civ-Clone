using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public class BarbarianFleeBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IUnitPositionCanon     UnitPositionCanon;
        private IUnitStrengthEstimator UnitStrengthEstimator;
        private IBarbarianConfig       BarbarianConfig;
        private IHexPathfinder         HexPathfinder;
        private IHexGrid               Grid;
        private IBarbarianBrainTools   BrainTools;
        private DiContainer            Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianFleeBrain(
            IUnitPositionCanon unitPositionCanon, IUnitStrengthEstimator unitStrengthEstimator,
            IBarbarianConfig barbarianConfig, IHexPathfinder hexPathfiner, IHexGrid grid,
            IBarbarianBrainTools brainTools, DiContainer container
        ) {
            UnitPositionCanon     = unitPositionCanon;
            UnitStrengthEstimator = unitStrengthEstimator;
            BarbarianConfig       = barbarianConfig;
            HexPathfinder         = hexPathfiner;
            Grid                  = grid;
            BrainTools            = brainTools;
            Container             = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            float unitStrength = UnitStrengthEstimator.EstimateUnitDefensiveStrength(unit, unitLocation);

            float logisticX = maps.EnemyPresence[unitLocation.Index] - maps.AllyPresence[unitLocation.Index];

            var logistic = AIMath.NormalizedLogisticCurve(unitStrength, BarbarianConfig.FleeUtilityLogisticSlope, logisticX);

            return logistic;
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var reachableCellsByDistance = HexPathfinder.GetAllCellsReachableIn(
                unitLocation, unit.CurrentMovement, UnitPositionCanon.GetPathfindingCostFunction(unit, false),
                Grid.Cells
            );

            var validCandidates = reachableCellsByDistance.Keys.Where(cell => UnitPositionCanon.CanPlaceUnitAtLocation(unit, cell, false));

            IHexCell bestCandidate = validCandidates.MaxElement(BrainTools.GetFleeWeightFunction(unit, maps));

            var retval = new List<IUnitCommand>();

            if(bestCandidate != null) {
                var moveCommand = Container.Instantiate<MoveUnitCommand>();

                moveCommand.UnitToMove      = unit;
                moveCommand.DesiredLocation = bestCandidate;

                retval.Add(moveCommand);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
