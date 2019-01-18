using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianConfig {

        #region properties

        float WanderSelectionWeight_Distance { get; }
        float WanderSelectionWeight_Allies   { get; }
        float WanderSelectionWeight_Enemies  { get; }

        int MinEncampmentsPerPlayer { get; }
        int MaxEncampmentsPerPlayer { get; }

        float BaseEncampmentSpawnWeight  { get; }
        float AllyEncampmentSpawnWeight  { get; }
        float EnemyEncampmentSpawnWeight { get; }

        int MinEncampmentSpawnProgress { get; }
        int MaxEncampmentSpawnProgress { get; }

        int ProgressNeededForUnitSpawn { get; }

        float WaterSpawnChance { get; }

        IEnumerable<IUnitTemplate> UnitsToSpawn { get; }

        float EncampmentBounty { get; }



        int DefendEncampmentRadius { get; }

        float WanderGoalUtility            { get; }
        float StayInEncampmentUtility      { get; }
        float HeadTowardsEncampmentUtility { get; }
        float PillageUtilityCoefficient    { get; }
        float CaptureCivilianUtility       { get; }
        float FleeUtilityLogisticSlope     { get; }
        float AttackUtilityLogisticsSlope  { get; }

        #endregion

    }

}
