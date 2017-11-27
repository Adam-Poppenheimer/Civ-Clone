using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.GameMap {

    public interface ITileConfig {

        #region properties

        Material DesertMaterial { get; }
        Material GrasslandMaterial { get; }
        Material PlainsMaterial { get; }
        ReadOnlyCollection<TerrainType> UnoccupiableTerrains { get; }

        #endregion

        #region methods

        Material GetTerrainMaterial(TerrainType terrain);

        #endregion

    }

}