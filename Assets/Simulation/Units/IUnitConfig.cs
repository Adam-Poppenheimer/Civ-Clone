using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnitConfig {

        #region properties

        int MaxHealth { get; }

        ReadOnlyCollection<float> TerrainDefensiveness { get; }
        ReadOnlyCollection<float> FeatureDefensiveness { get; }
        ReadOnlyCollection<float> ShapeDefensiveness   { get; }

        float RiverCrossingAttackModifier { get; }

        float CombatBaseDamage { get; }

        #endregion

    }

}
