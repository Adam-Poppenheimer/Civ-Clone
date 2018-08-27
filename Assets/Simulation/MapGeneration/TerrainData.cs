using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public delegate bool SeedFilter(
        IHexCell cell, IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCell
    );

    [Serializable]
    public struct TerrainData {

        #region instance fields and properties

        public int Percentage {
            get { return _percentage; }
        }
        [SerializeField, Range(0, 100)] private int _percentage;

        public int SeedCount {
            get { return _seedCount; }
        }
        [SerializeField] private int _seedCount;

        #endregion

    }

}
