using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGridConfig {

        #region properties

        int RandomSeed { get; }

        ReadOnlyCollection<ResourceSummary> TerrainYields { get; }

        ReadOnlyCollection<ResourceSummary> FeatureYields { get; }

        int BaseLandMoveCost { get; }
        int WaterMoveCost    { get; }

        ReadOnlyCollection<int> FeatureMoveCosts { get; }

        int SlopeMoveCost { get; }

        #endregion

    }

}