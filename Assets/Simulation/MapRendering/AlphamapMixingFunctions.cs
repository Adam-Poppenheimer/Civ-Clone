using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class AlphamapMixingFunctions : IAlphamapMixingFunctions {

        #region instance fields and properties

        private ICellAlphamapLogic CellAlphamapLogic;

        #endregion

        #region constructors

        [Inject]
        public AlphamapMixingFunctions(ICellAlphamapLogic cellAlphamapLogic) {
            CellAlphamapLogic = cellAlphamapLogic;
        }

        #endregion

        #region instance methods

        #region from IAlphamapMixingFunctions

        public float[] Selector(Vector3 position, IHexCell cell, HexDirection sextant, float weight) {
            var retval = CellAlphamapLogic.GetAlphamapForPositionForCell(position, cell, sextant);

            for(int i = 0; i < retval.Length; i++) {
                retval[i] *= weight;
            }

            return retval;
        }

        public float[] Aggregator(float[] alphamapOne, float[] alphamapTwo) {
            var newMap = new float[alphamapOne.Length];

            for(int i = 0; i < alphamapOne.Length; i++) {
                newMap[i] = alphamapOne[i] + alphamapTwo[i];
            }

            return newMap;
        }

        #endregion

        #endregion

    }

}
