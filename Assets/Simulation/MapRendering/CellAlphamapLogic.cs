using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.MapRendering {

    public class CellAlphamapLogic : ICellAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig          RenderConfig;
        private IHexGrid                  Grid;
        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        public CellAlphamapLogic(
            IMapRenderConfig renderConfig, IHexGrid grid, IImprovementLocationCanon improvementLocationCanon
        ) {
            RenderConfig             = renderConfig;
            Grid                     = grid;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICellAlphamapLogic

        public float[] GetAlphamapForPositionForCell(Vector3 position, IHexCell center, HexDirection sextant) {
            var retval = new float[RenderConfig.MapTextures.Count()];

            var improvementsAt = ImprovementLocationCanon.GetPossessionsOfOwner(center);

            if(center.Terrain.IsWater()) {
                retval[RenderConfig.SeaFloorTextureIndex] = 1f;

            }else if(center.Shape == CellShape.Mountains) {
                retval[RenderConfig.MountainTextureIndex] = 1f;

            }else if(improvementsAt.Any(improvement => improvement.Template.OverridesTerrain)) {
                int newIndex = improvementsAt.First(improvement => improvement.Template.OverridesTerrain).Template.OverridingTerrainIndex;

                retval[newIndex] = 1f;

            } else {
                retval[(int)center.Terrain] = 1f;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
