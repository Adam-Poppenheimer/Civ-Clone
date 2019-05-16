using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.AI {

    public class AttackUnitCommand : IUnitCommand {

        #region instance fields and properties

        #region from IUnitCommand

        public CommandStatus Status { get; private set; }

        #endregion

        public IUnit      Attacker         { get; set; }
        public IHexCell   LocationToAttack { get; set; }
        public CombatType CombatType       { get; set; }




        private ICombatExecuter       CombatExecuter;
        private IUnitAttackOrderLogic AttackOrderLogic;

        #endregion

        #region constructors

        [Inject]
        public AttackUnitCommand(
            ICombatExecuter combatExecuter, IUnitAttackOrderLogic attackOrderLogic
        ) {
            CombatExecuter   = combatExecuter;
            AttackOrderLogic = attackOrderLogic;
        }

        #endregion

        #region instance methods

        #region from IUnitCommand

        public void StartExecution() {
            if(Attacker == null) {
                throw new InvalidOperationException("Cannot execute while Attacker is null");
            }

            if(LocationToAttack == null) {
                throw new InvalidOperationException("Cannot execute while LocationToAttack is null");
            }

            Status = CommandStatus.Running;

            var bestDefender = AttackOrderLogic.GetNextAttackTargetOnCell(LocationToAttack);

            if(bestDefender == null) {
                Status = CommandStatus.Failed;

            }else if(CombatType == CombatType.Melee) {
                if( CombatExecuter.CanPerformMeleeAttack(Attacker, bestDefender)) {
                    CombatExecuter.PerformMeleeAttack(
                        Attacker, bestDefender, () => Status = CommandStatus.Succeeded, () => Status = CommandStatus.Failed
                    );
                }else {
                    Status = CommandStatus.Failed;
                }
            }else {
                if( CombatExecuter.CanPerformRangedAttack(Attacker, bestDefender)) {
                    CombatExecuter.PerformRangedAttack   (Attacker, bestDefender);

                    Status = CommandStatus.Succeeded;
                }else {
                    Status = CommandStatus.Failed;
                }
            }
        }

        #endregion

        #endregion

    }

}
