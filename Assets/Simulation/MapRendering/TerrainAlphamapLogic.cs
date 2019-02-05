using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainAlphamapLogic : ITerrainAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public TerrainAlphamapLogic(IMapRenderConfig renderConfig, IHexGrid grid) {
            RenderConfig = renderConfig;
            Grid         = grid;
        }

        #endregion

        #region instance methods

        #region from ITerrainAlphamapLogic

        public float[] GetAlphamapForPosition(Vector3 position) {
            var retval = new float[RenderConfig.MapTextures.Count()];

            if(Grid.HasCellAtLocation(position)) {
                var cellAt = Grid.GetCellAtLocation(position);

                retval[(int)cellAt.Terrain] = 1f;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
