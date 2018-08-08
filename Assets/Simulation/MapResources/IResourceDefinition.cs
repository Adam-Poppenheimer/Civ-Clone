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

        int GrasslandWeight    { get; }
        int PlainsWeight       { get; }
        int DesertWeight       { get; }
        int FloodPlainsWeight  { get; }
        int TundraWeight       { get; }
        int SnowWeight         { get; }
        int ShallowWaterWeight { get; }

        int HillWeight         { get; }

        int ForestWeight       { get; }
        int JungleWeight       { get; }
        int MarshWeight        { get; }

        int SelectionWeight { get; }

        Transform AppearancePrefab { get; }

        Sprite Icon { get; }

        #endregion

    }

}
