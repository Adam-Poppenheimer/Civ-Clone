using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class OrientationTriangulator : IOrientationTriangulator {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public OrientationTriangulator(IMapRenderConfig renderConfig) {
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IOrientationTriangulator

        public void TriangulateOrientation(IHexCell cell, IHexMesh orientationMesh) {
            short indexOffset = (short)(cell.Index + 1);

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                byte[] rg = BitConverter.GetBytes(indexOffset);
                byte b  = (byte)direction;

                var cellColor = new Color32(rg[0], rg[1], b, 0);

                orientationMesh.AddTriangle(
                    Vector3.down + cell.AbsolutePosition,
                    Vector3.down + cell.AbsolutePosition + RenderConfig.GetFirstCorner (direction),
                    Vector3.down + cell.AbsolutePosition + RenderConfig.GetSecondCorner(direction)
                );
                orientationMesh.AddTriangleColor(cellColor);
            }
        }

        #endregion

        #endregion

    }

}
