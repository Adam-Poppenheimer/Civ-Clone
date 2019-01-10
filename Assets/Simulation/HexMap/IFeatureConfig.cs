using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IFeatureConfig {

        #region properties

        ReadOnlyCollection<Transform> ForestTreePrefabs { get; }
        ReadOnlyCollection<Transform> JungleTreePrefabs { get; }
        ReadOnlyCollection<Transform> BuildingPrefabs   { get; }
        ReadOnlyCollection<Transform> RuinsPrefabs      { get; }
        ReadOnlyCollection<Transform> EncampmentPrefabs { get; }

        float TreeAppearanceChance        { get; }
        float BuildingAppearanceChance    { get; }
        float ResourceAppearanceChance    { get; }
        float ImprovementAppearanceChance { get; }
        float RuinsAppearanceChance       { get; }
        float EncampmentAppearanceChance  { get; }

        int GuaranteedTreeModulo        { get; }
        int GuaranteedBuildingModulo    { get; }
        int GuaranteedResourceModulo    { get; }
        int GuaranteedImprovementModulo { get; }
        int GuaranteedRuinsModulo       { get; }
        int GuaranteedEncampmentModulo  { get; }

        #endregion

    }

}
