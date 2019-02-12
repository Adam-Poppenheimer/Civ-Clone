using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public delegate T DataSelectorCallback<T>(Vector3 position, IHexCell cell, HexDirection sextant, float weight);

    public interface ITerrainMixingLogic {

        #region methods

        T GetMixForEdgeAtPoint<T>(
            IHexCell center, IHexCell right, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        );

        T GetMixForPreviousCornerAtPoint<T>(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        );

        T GetMixForNextCornerAtPoint<T>(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        );

        #endregion

    }

}
