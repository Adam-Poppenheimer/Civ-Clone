using System.Collections.ObjectModel;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    public interface IMapSizeCategory {

        #region properties

        string name { get; }

        Vector2 DimensionsInChunks { get; }

        int IdealCivCount { get; }

        ReadOnlyCollection<int> ValidCivCounts { get; }

        #endregion

    }

}