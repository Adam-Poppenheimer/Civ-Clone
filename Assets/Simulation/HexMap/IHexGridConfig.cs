using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGridConfig {

        #region properties

        int RandomSeed { get; }

        ReadOnlyCollection<ResourceSummary> TerrainYields { get; }
        ReadOnlyCollection<ResourceSummary> FeatureYields { get; }
        ReadOnlyCollection<ResourceSummary> ShapeYields   { get; }

        int BaseLandMoveCost { get; }
        int WaterMoveCost    { get; }

        ReadOnlyCollection<int> FeatureMoveCosts { get; }
        ReadOnlyCollection<int> ShapeMoveCosts   { get; }

        int SlopeMoveCost { get; }

        float RoadMoveCostMultiplier { get; }

        #endregion

    }

}