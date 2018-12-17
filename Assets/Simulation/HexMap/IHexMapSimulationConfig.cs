using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexMapSimulationConfig {

        #region properties

        int SlopeMoveCost { get; }
        int CityMoveCost  { get; }

        float RoadMoveCostMultiplier { get; }

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
        int GetBaseMoveCostOfFeature   (CellFeature    feature);

        #endregion

    }

}