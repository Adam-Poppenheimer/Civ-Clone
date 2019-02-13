using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IAlphamapMixingFunctions {

        #region methods

        float[] Selector(Vector3 position, IHexCell cell, HexDirection sextant, float weight);

        float[] Aggregator(float[] alphamapOne, float[] alphamapTwo);
        
        #endregion

    }
}