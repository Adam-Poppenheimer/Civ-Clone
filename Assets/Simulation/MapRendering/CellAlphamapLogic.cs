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

        public float[] GetAlphamapForPointForCell(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            Profiler.BeginSample("CellAlphamapLogic.GetAlphamapForPointForCell()");

            var retval = new float[RenderConfig.MapTextures.Count()];

            var improvementsAt = ImprovementLocationCanon.GetPossessionsOfOwner(cell);

            if(cell.Terrain.IsWater()) {
                retval[RenderConfig.SeaFloorTextureIndex] = 1f;

            }else if(cell.Shape == CellShape.Mountains) {
                retval[RenderConfig.MountainTextureIndex] = 1f;

            }else if(improvementsAt.Any(improvement => improvement.Template.OverridesTerrain)) {
                int newIndex = improvementsAt.First(improvement => improvement.Template.OverridesTerrain).Template.OverridingTerrainIndex;

                retval[newIndex] = 1f;

            } else {
                retval[(int)cell.Terrain] = 1f;
            }

            Profiler.EndSample();
            return retval;
        }

        #endregion

        #endregion
        
    }

}
