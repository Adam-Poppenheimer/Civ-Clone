using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.AI {

    public class MoveUnitCommand : IUnitCommand {

        #region instance fields and properties

        #region from IUnitCommand

        public CommandStatus Status { get; private set; }

        #endregion

        public IUnit    UnitToMove      { get; set; }
        public IHexCell DesiredLocation { get; set; } 




        private IHexPathfinder     HexPathfinder;
        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public MoveUnitCommand(
            IHexPathfinder hexPathfinder, IUnitPositionCanon unitPositionCanon
        ) {
            HexPathfinder     = hexPathfinder;
            UnitPositionCanon = unitPositionCanon;

            Status = CommandStatus.NotStarted;
        }

        #endregion

        #region instance methods

        #region from IUnitCommand

        public void StartExecution() {
            Status = CommandStatus.Running;

            UnitToMove.CurrentPath = null;

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(UnitToMove);

            var pathTo = HexPathfinder.GetShortestPathBetween(
                unitLocation, DesiredLocation,
                (current, next) => UnitPositionCanon.GetTraversalCostForUnit(UnitToMove, current, next, false)
            );

            if(pathTo != null) {
                UnitToMove.CurrentPath = pathTo;

                UnitToMove.PerformMovement(false, OnUnitFinishedMovement);
            }else {
                Status = CommandStatus.Failed;
            }
        }

        #endregion

        private void OnUnitFinishedMovement() {
            Status =
                UnitPositionCanon.GetOwnerOfPossession(UnitToMove) == DesiredLocation
                ? CommandStatus.Succeeded
                : CommandStatus.Failed;
        }

        #endregion

    }

}
