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
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public OrientationTriangulator(
            IMapRenderConfig renderConfig, IHexGrid grid
        ) {
            RenderConfig = renderConfig;
            Grid         = grid;
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
                    cell.AbsolutePosition,
                    cell.AbsolutePosition + RenderConfig.GetFirstCorner (direction),
                    cell.AbsolutePosition + RenderConfig.GetSecondCorner(direction)
                );

                orientationMesh.AddTriangleColor(cellColor);
            }
        }

        public PointOrientationData GetDataFromColor(Color32 color) {
            int index = BitConverter.ToInt16(new byte[] { color.r, color.g }, 0) - 1;

            var center = index >= 0 && index < Grid.Cells.Count ? Grid.Cells[index] : null;

            HexDirection sextant = (HexDirection)color.b;

            var retval = new PointOrientationData() {
                IsOnGrid = center != null,
                Sextant  = sextant,

                Center = center,
                CenterWeight = 1f
            };

            return retval;
        }

        #endregion

        #endregion

    }

}
