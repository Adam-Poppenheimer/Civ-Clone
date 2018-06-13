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

        ReadOnlyCollection<Transform> BuildingPrefabs { get; }

        float TreeAppearanceChance { get; }

        float BuildingAppearanceChance { get; }

        float ResourceAppearanceChance { get; }

        #endregion

    }

}
