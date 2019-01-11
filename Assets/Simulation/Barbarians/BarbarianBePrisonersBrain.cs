using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public class BarbarianBePrisonersBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IHexGrid                 Grid;
        private IHexPathfinder           HexPathfinder;
        private IUnitPositionCanon       UnitPositionCanon;
        private IEncampmentLocationCanon EncampmentLocationCanon;
        private DiContainer              Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianBePrisonersBrain(
            IHexGrid grid, IHexPathfinder hexPathfinder, IUnitPositionCanon unitPositionCanon,
            IEncampmentLocationCanon encampmentLocationCanon, DiContainer container
        ) {
            Grid                    = grid;
            HexPathfinder           = hexPathfinder;
            UnitPositionCanon       = unitPositionCanon;
            EncampmentLocationCanon = encampmentLocationCanon;
            Container               = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            return unit.Type == UnitType.Civilian ? 1f : 0f;
        }

         public List<IUnitCommand> GetCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            var retval = new List<IUnitCommand>();

            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            Dictionary<IHexCell, float> cellsByCost = HexPathfinder.GetCostToAllCells(
                unitPosition, (current, next) => UnitPositionCanon.GetTraversalCostForUnit(unit, current, next, false),
                Grid.Cells
            );

            IHexCell bestCandidate = null;
            float lowestCost = float.MaxValue;

            foreach(var candidate in cellsByCost.Keys) {
                if(!EncampmentLocationCanon.GetPossessionsOfOwner(candidate).Any()) {
                    continue;
                }

                if(bestCandidate == null || lowestCost > cellsByCost[candidate]) {
                    bestCandidate = candidate;
                    lowestCost = cellsByCost[candidate];
                }
            }

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
