using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public interface IResourceDefinition {

        #region properties

        string name { get; }

        YieldSummary BonusYieldBase { get; }

        YieldSummary BonusYieldWhenImproved { get; }

        ResourceType Type { get; }

        IImprovementTemplate Extractor { get; }

        Transform AppearancePrefab { get; }

        Sprite Icon { get; }

        float Score { get; }

        #endregion

        #region methods

        int GetWeightFromTerrain   (CellTerrain    terrain);
        int GetWeightFromShape     (CellShape      shape);
        int GetWeightFromVegetation(CellVegetation vegetation);

        #endregion

    }

}
