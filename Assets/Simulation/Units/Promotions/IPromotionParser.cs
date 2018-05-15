﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionParser {

        #region methods

        CombatInfo GetCombatInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType);

        MovementInfo GetMovementInfo(IUnit unit);

        VisionInfo GetVisionInfo(IUnit unit);

        HealingInfo GetHealingInfo(IUnit unit);

        #endregion

    }

}
