using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotion {

        #region properties

        string name { get; }

        string Description { get; }

        Sprite Icon { get; }

        #endregion

        #region methods

        void ModifyCombatInfoForAttacker(
            IUnit attacker, IUnit defender, IHexCell location,
            CombatType combatType, CombatInfo combatInfo
        );

        void ModifyCombatInfoForDefender(
            IUnit attacker, IUnit defender, IHexCell location,
            CombatType combatType, CombatInfo combatInfo
        );

        void ModifyMovementInfo(IUnit unit, MovementInfo movementInfo);

        #endregion

    }

}
