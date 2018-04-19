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

        #endregion

        #region methods

        float GetTerrainDefensiveness(TerrainType    terrain);        
        float GetShapeDefensiveness  (TerrainShape   shape);
        float GetFeatureDefensiveness(TerrainFeature feature);

        #endregion

    }

}
