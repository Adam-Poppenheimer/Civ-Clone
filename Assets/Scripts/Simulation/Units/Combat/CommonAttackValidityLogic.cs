using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.Units.Combat {

    public class CommonAttackValidityLogic : ICommonAttackValidityLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IWarCanon                                     WarCanon;

        #endregion

        #region constructors

        [Inject]
        public CommonAttackValidityLogic(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, IWarCanon warCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            WarCanon            = warCanon;
        }

        #endregion

        #region instance methods

        #region from ICommonAttackValidityLogic

        public bool DoesAttackMeetCommonConditions(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(attacker == defender) {
                return false;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);
            var defenderOwner = UnitPossessionCanon.GetOwnerOfPossession(defender);

            if(attackerOwner == defenderOwner) {
                return false;
            }

            if(!WarCanon.AreAtWar(attackerOwner, defenderOwner)) {
                return false;
            }

            if(!attacker.CanAttack) {
                return false;
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
