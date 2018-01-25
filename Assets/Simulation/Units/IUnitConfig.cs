using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnitConfig {

        #region properties

        int MaxHealth { get; }

        ReadOnlyCollection<float> TerrainMeleeDefensiveness { get; }
        ReadOnlyCollection<float> TerrainRangedDefensiveness { get; }

        ReadOnlyCollection<float> FeatureMeleeDefensiveness { get; }
        ReadOnlyCollection<float> FeatureRangedDefensiveness { get; }

        float RiverCrossingAttackModifier { get; }

        float CombatBaseDamage { get; }

        int VisionRange { get; }

        #endregion

    }

}
