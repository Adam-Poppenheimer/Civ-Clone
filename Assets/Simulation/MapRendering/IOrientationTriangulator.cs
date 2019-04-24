﻿using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IOrientationTriangulator {

        #region methods

        void TriangulateOrientation(IHexCell cell, IHexMesh orientationMesh);

        PointOrientationData GetDataFromColor(Color32 color);

        #endregion

    }

}