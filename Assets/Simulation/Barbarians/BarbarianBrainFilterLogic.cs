using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Barbarians {

    public class BarbarianBrainFilterLogic : IBarbarianBrainFilterLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;
        private ICombatExecuter    CombatExecuter;

        #endregion

        #region constructors

        [Inject]
        public BarbarianBrainFilterLogic(
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter
        ) {
            UnitPositionCanon = unitPositionCanon;
            CombatExecuter    = combatExecuter;
        }

        #endregion

        #region instance methods

        #region from IBarbarianBrainFilterLogic

        public Func<IHexCell, bool> GetCaptureCivilianFilter(IUnit captor) {
            return delegate(IHexCell cell) {
                var captiveCandidates = UnitPositionCanon.GetPossessionsOfOwner(cell);

                return captiveCandidates.Any() && captiveCandidates.All(
                    captiveCandidate => captiveCandidate.Type.IsCivilian() && CombatExecuter.CanPerformMeleeAttack(captor, captiveCandidate)
                );
            };
        }

        public Func<IHexCell, bool> GetMeleeAttackFilter(IUnit attacker) {
            return delegate(IHexCell candidate) {
                var unitsAt = UnitPositionCanon.GetPossessionsOfOwner(candidate);

                return unitsAt.Any(unit => CombatExecuter.CanPerformMeleeAttack(attacker, unit));
            };
        }

        public Func<IHexCell, bool> GetRangedAttackFilter(IUnit attacker) {
            return delegate(IHexCell candidate) {
                var unitsAt = UnitPositionCanon.GetPossessionsOfOwner(candidate);

                return unitsAt.Any(unit => CombatExecuter.CanPerformRangedAttack(attacker, unit));
            };
        }

        #endregion

        #endregion

    }

}
