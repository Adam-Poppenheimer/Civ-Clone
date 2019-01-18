using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public class BarbarianWanderBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IHexGrid                         Grid;
        private IUnitPositionCanon               UnitPositionCanon;
        private DiContainer                      Container;
        private IWeightedRandomSampler<IHexCell> CellRandomSampler;
        private IBarbarianBrainWeightLogic             BrainTools;
        private IBarbarianConfig                 BarbarianConfig;

        #endregion

        #region constructors

        [Inject]
        public BarbarianWanderBrain(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, DiContainer container,
            IWeightedRandomSampler<IHexCell> cellRandomSampler, IBarbarianBrainWeightLogic brainTools,
            IBarbarianConfig barbarianConfig
        ) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
            Container         = container;
            CellRandomSampler = cellRandomSampler;
            BrainTools        = brainTools;
            BarbarianConfig   = barbarianConfig;
        }

        #endregion

        #region instance methods

        #region from IBarbarianWanderBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            if(unit.Type.IsCivilian()) {
                return 0f;
            }else {
                return BarbarianConfig.WanderGoalUtility;
            }
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            var retval = new List<IUnitCommand>();

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var nearbyCells = Grid.GetCellsInRadius(unitLocation, Mathf.RoundToInt(unit.MaxMovement));

            var bestCandidate = CellRandomSampler.SampleElementsFromSet(
                nearbyCells, 1, BrainTools.GetWanderWeightFunction(unit, maps)
            ).FirstOrDefault();

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
