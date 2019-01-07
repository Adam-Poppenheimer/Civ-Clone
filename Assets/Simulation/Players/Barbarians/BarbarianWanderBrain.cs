using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Players.Barbarians {

    public class BarbarianWanderBrain : IBarbarianWanderBrain {

        #region instance fields and properties

        private IHexGrid                         Grid;
        private IUnitPositionCanon               UnitPositionCanon;
        private DiContainer                      Container;
        private IWeightedRandomSampler<IHexCell> CellRandomSampler;
        private IBarbarianBrainTools             BrainTools;

        #endregion

        #region constructors

        [Inject]
        public BarbarianWanderBrain(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, DiContainer container,
            IWeightedRandomSampler<IHexCell> cellRandomSampler, IBarbarianBrainTools brainTools
        ) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
            Container         = container;
            CellRandomSampler = cellRandomSampler;
            BrainTools        = brainTools;
        }

        #endregion

        #region instance methods

        #region from IBarbarianWanderBrain

        public List<IUnitCommand> GetWanderCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
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
