using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexMapConfig {

        #region properties

        int RandomSeed { get; }

        int SlopeMoveCost { get; }
        int CityMoveCost  { get; }

        float RoadMoveCostMultiplier { get; }

        int WaterLevel { get; }

        #endregion

        #region methods

        ResourceSummary GetYieldOfTerrain(TerrainType    terrain);
        ResourceSummary GetYieldOfFeature(TerrainFeature feature);
        ResourceSummary GetYieldOfShape  (TerrainShape   shape);

        int GetBaseMoveCostOfTerrain(TerrainType    terrain);
        int GetBaseMoveCostOfShape  (TerrainShape   shape);
        int GetBaseMoveCostOfFeature(TerrainFeature feature);

        int GetFoundationElevationForTerrain(TerrainType  terrain);
        int GetEdgeElevationForShape        (TerrainShape shape);
        int GetPeakElevationForShape        (TerrainShape shape);

        #endregion

    }

}