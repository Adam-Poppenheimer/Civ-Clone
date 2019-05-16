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

    public class BarbarianPillageBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IUnitPositionCanon         UnitPositionCanon;
        private IHexGrid                   Grid;
        private IBarbarianBrainWeightLogic WeightLogic;
        private IBarbarianUtilityLogic     UtilityLogic;
        private DiContainer                Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianPillageBrain(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, IBarbarianBrainWeightLogic weightLogic,
            IBarbarianUtilityLogic utilityLogic, DiContainer container
        ) {
            UnitPositionCanon = unitPositionCanon;
            Grid              = grid;
            WeightLogic       = weightLogic;
            UtilityLogic      = utilityLogic;
            Container         = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return Grid.GetCellsInRadius(unitLocation, Mathf.RoundToInt(unit.MaxMovement)).Max(
                UtilityLogic.GetPillageUtilityFunction(unit, maps)
            );
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            var retval = new List<IUnitCommand>();

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var nearbyCells = Grid.GetCellsInRadius(unitLocation, Mathf.RoundToInt(unit.MaxMovement));

            IHexCell bestCandidate = null;
            int bestWeight = int.MinValue;

            var weightFunction = WeightLogic.GetPillageWeightFunction(unit, maps);

            foreach(var candidate in nearbyCells) {
                int candidateWeight = weightFunction(candidate);

                if(bestCandidate == null || candidateWeight > bestWeight) {
                    bestCandidate = candidate;
                    bestWeight = candidateWeight;
                }
            }

            if(bestCandidate != null) {
                var moveCommand = Container.Instantiate<MoveUnitCommand>();

                moveCommand.UnitToMove      = unit;
                moveCommand.DesiredLocation = bestCandidate;

                retval.Add(moveCommand);

                var pillageCommand = Container.Instantiate<PillageUnitCommand>();

                pillageCommand.Pillager = unit;

                retval.Add(pillageCommand);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
