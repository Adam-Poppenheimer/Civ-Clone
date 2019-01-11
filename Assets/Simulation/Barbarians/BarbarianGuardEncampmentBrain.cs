using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public class BarbarianGuardEncampmentBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IUnitPositionCanon       UnitPositionCanon;
        private IHexGrid                 Grid;
        private IBarbarianConfig         BarbarianConfig;
        private IEncampmentLocationCanon EncampmentLocationCanon;
        private DiContainer              Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianGuardEncampmentBrain(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, IBarbarianConfig barbarianConfig,
            IEncampmentLocationCanon encampmentLocationCanon, DiContainer container
        ) {
            UnitPositionCanon       = unitPositionCanon;
            Grid                    = grid;
            BarbarianConfig         = barbarianConfig;
            EncampmentLocationCanon = encampmentLocationCanon;
            Container               = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            if(EncampmentLocationCanon.GetPossessionsOfOwner(unitPosition).Any()) {
                return BarbarianConfig.StayInEncampmentUtility;

            }else if(Grid.GetCellsInRadius(unitPosition, BarbarianConfig.DefendEncampmentRadius).Any(HasUndefendedEncampment)) {
                return BarbarianConfig.HeadTowardsEncampmentUtility;

            }else {
                return 0f;
            }
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps) {
            var retval = new List<IUnitCommand>();

            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            if(EncampmentLocationCanon.GetPossessionsOfOwner(unitPosition).Any()) {
                return retval;
            }

            IHexCell bestCandidate = Grid.GetCellsInRadius(unitPosition, BarbarianConfig.DefendEncampmentRadius)
                                         .FirstOrDefault(HasUndefendedEncampment);

            if(bestCandidate != null) {
                var moveCommand = Container.Instantiate<MoveUnitCommand>();

                moveCommand.DesiredLocation = bestCandidate;
                moveCommand.UnitToMove      = unit;

                retval.Add(moveCommand);
            }

            return retval;
        }

        #endregion

        private bool HasUndefendedEncampment(IHexCell cell) {
            return EncampmentLocationCanon.GetPossessionsOfOwner(cell).Any()
                && !UnitPositionCanon     .GetPossessionsOfOwner(cell).Any();
        }

        #endregion

        
    }

}
