using System.Collections.Generic;

namespace Assets.Simulation.Units.Combat {

    public interface IUnitCombatSummary {

        #region properties

        bool CanAttackAfterAttacking { get; }
        bool CanMoveAfterAttacking   { get; }
        
        bool IgnoresAmphibiousPenalty     { get; }
        bool IgnoresDefensiveTerrainBonus { get; }
        bool IgnoresLineOfSight           { get; }

        int BonusRange { get; }

        float GoldRaidingPercentage { get; }

        IEnumerable<ICombatModifier> ModifiersWhenAttacking { get; }
        IEnumerable<ICombatModifier> ModifiersWhenDefending { get; }

        #endregion

    }

}