using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Assets.Simulation.Units.Promotions {

    public enum PromotionArgType {
        [Description("Combat Strength")] CombatStrength = 0,

        [Description("When Defending")] WhenDefending = 1,
        [Description("When Attacking")] WhenAttacking = 2,

        [Description("On Flat Terrain")]  OnFlatTerrain  = 3,
        [Description("On Rough Terrain")] OnRoughTerrain = 4,

        [Description("In Forest And Jungle")] InForestAndJungle = 5,

        [Description("Against Unit Type")] AgainstUnitType = 6,

        [Description("Melee")]  Melee  = 7,
        [Description("Ranged")] Ranged = 8,

        [Description("Can Move After Attacking")]   CanMoveAfterAttacking    = 9,
        [Description("Can Attack After Attacking")] CanAttackAfterAttacking  = 10,
        [Description("Ignores Amphibious Penalty")] IgnoresAmphibiousPenalty = 11,

        [Description("No Defensive Terrain Bonuses")] NoDefensiveTerrainBonuses = 12,

        [Description("Has Rough Terrain Penalty")] HasRoughTerrainPenalty = 13,
        [Description("Ignores Terrain Costs")]     IgnoresTerrainCosts    = 14,
    }

}
