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

        int WaterTerrainIndex    { get; }
        int MountainTerrainIndex { get; }

        int MaxElevation { get; }

        #endregion

        #region methods

        bool DoesFeatureOverrideYield(CellFeature feature);

        YieldSummary GetYieldOfTerrain   (CellTerrain    terrain);
        YieldSummary GetYieldOfVegetation(CellVegetation vegetation);
        YieldSummary GetYieldOfShape     (CellShape      shape);
        YieldSummary GetYieldOfFeature   (CellFeature    feature);

        int GetBaseMoveCostOfTerrain   (CellTerrain    terrain);
        int GetBaseMoveCostOfShape     (CellShape      shape);
        int GetBaseMoveCostOfVegetation(CellVegetation vegetation);

        int GetFoundationElevationForTerrain(CellTerrain terrain);
        int GetEdgeElevationForShape        (CellShape shape);
        int GetPeakElevationForShape        (CellShape shape);

        #endregion

    }

}