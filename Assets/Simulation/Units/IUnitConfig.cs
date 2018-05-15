using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitConfig {

        #region properties

        int MaxHealth { get; }

        float RiverCrossingAttackModifier { get; }

        float CombatBaseDamage { get; }

        float TravelSpeedPerSecond { get; }

        float RotationSpeedPerSecond { get; }

        int NextLevelExperienceCoefficient { get; }

        int MaxLevel { get; }

        int MeleeAttackerExperience { get; }
        int MeleeDefenderExperience { get; }

        int RangedAttackerExperience { get; }
        int RangedDefenderExperience { get; }

        int WoundedThreshold { get; }

        int ForeignLandHealingPerTurn    { get; }
        int FriendlyLandHealingPerTurn   { get; }
        int GarrisonedLandHealingPerTurn { get; }

        int ForeignNavalHealingPerTurn    { get; }
        int FriendlyNavalHealingPerTurn   { get; }
        int GarrisonedNavalHealingPerTurn { get; }

        float CityRepairPercentPerTurn { get; }

        #endregion

        #region methods

        float GetTerrainDefensiveness(TerrainType    terrain);        
        float GetShapeDefensiveness  (TerrainShape   shape);
        float GetFeatureDefensiveness(TerrainFeature feature);

        #endregion

    }

}
