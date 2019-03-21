using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MapCollisionLogic : IMapCollisionLogic {

        #region instance fields and properties

        private Vector3 RaycastOffset;



        private IHexGrid         Grid;
        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public MapCollisionLogic(IHexGrid grid, IMapRenderConfig renderConfig) {
            Grid         = grid;
            RenderConfig = renderConfig;

            RaycastOffset = Vector3.up * RenderConfig.TerrainMaxY * 1.5f;
        }

        #endregion

        #region instance methods

        #region from IMapCollisionLogic

        public Vector3 GetNearestMapPointToPoint(Vector3 point) {
            RaycastHit hitInfo;

            if(Physics.Raycast(point + RaycastOffset, Vector3.down, out hitInfo, RenderConfig.TerrainMaxY * 2, RenderConfig.MapCollisionMask)) {
                return hitInfo.point;
            }else {
                return point;
            }
        }

        #endregion

        #endregion

    }

}
