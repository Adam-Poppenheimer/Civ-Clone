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

            ReusedAlphamap = new float[RenderConfig.MapTextures.Count()];
        }

        #endregion

        #region instance methods

        #region from ICellAlphamapLogic

        /* As elsewhere in the code, we reuse the same array here
         * to save on memory requirements. Make sure that multiple
         * uses of GetAlphamapForCell are not being used
         * at the same time.
         */ 

        public float[] ReusedAlphamap;

        public float[] GetAlphamapForCell(IHexCell cell, HexDirection sextant) {
            Profiler.BeginSample("CellAlphamapLogic.GetAlphamapForPointForCell()");

            for(int i = 0; i < ReusedAlphamap.Length; i++) {
                ReusedAlphamap[i] = 0f;
            }

            var improvementsAt = ImprovementLocationCanon.GetPossessionsOfOwner(cell);

            if(cell.Terrain.IsWater()) {
                ReusedAlphamap[RenderConfig.SeaFloorTextureIndex] = 1f;

            }else if(cell.Shape == CellShape.Mountains) {
                ReusedAlphamap[RenderConfig.MountainTextureIndex] = 1f;

            }else if(improvementsAt.Any(improvement => improvement.Template.OverridesTerrain)) {
                int newIndex = improvementsAt.First(improvement => improvement.Template.OverridesTerrain).Template.OverridingTerrainIndex;

                ReusedAlphamap[newIndex] = 1f;

            } else {
                ReusedAlphamap[(int)cell.Terrain] = 1f;
            }

            Profiler.EndSample();
            return ReusedAlphamap;
        }

        #endregion

        #endregion
        
    }

}
