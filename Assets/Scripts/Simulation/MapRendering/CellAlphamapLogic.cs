using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.MapRendering {

    public class CellAlphamapLogic : ICellAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig          RenderConfig;
        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        public CellAlphamapLogic(
            IMapRenderConfig renderConfig, IImprovementLocationCanon improvementLocationCanon
        ) {
            RenderConfig             = renderConfig;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICellAlphamapLogic

        public void GetAlphamapForCell(float[] returnedAlphamap, IHexCell cell) {
            for(int i = 0; i < returnedAlphamap.Length; i++) {
                returnedAlphamap[i] = 0f;
            }

            var improvementsAt = ImprovementLocationCanon.GetPossessionsOfOwner(cell);

            if(cell.Terrain.IsWater()) {
                returnedAlphamap[RenderConfig.SeaFloorTextureIndex] = 1f;

            }else if(cell.Shape == CellShape.Mountains) {
                returnedAlphamap[RenderConfig.MountainTextureIndex] = 1f;

            }else if(improvementsAt.Any(improvement => improvement.Template.OverridesTerrain)) {
                int newIndex = improvementsAt.First(improvement => improvement.Template.OverridesTerrain).Template.OverridingTerrainIndex;

                returnedAlphamap[newIndex] = 1f;

            } else {
                returnedAlphamap[(int)cell.Terrain] = 1f;
            }
        }

        #endregion

        #endregion
        
    }

}
