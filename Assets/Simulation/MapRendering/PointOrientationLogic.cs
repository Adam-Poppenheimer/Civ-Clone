using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationLogic : IPointOrientationLogic {

        #region instance fields and properties

        private float SolidRadiusSq;



        private IHexGrid                        Grid;
        private IPointOrientationInSextantLogic PointOrientationInSextantLogic;
        private IMapRenderConfig                RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(
            IHexGrid grid, IPointOrientationInSextantLogic pointOrientationInSextantLogic,
            IMapRenderConfig renderConfig
        ) {
            Grid                           = grid;
            PointOrientationInSextantLogic = pointOrientationInSextantLogic;
            RenderConfig                   = renderConfig;

            SolidRadiusSq = RenderConfig.SolidFactor * RenderConfig.InnerRadius;
            SolidRadiusSq *= SolidRadiusSq;
        }

        #endregion

        #region instance methods

        #region from IPointOrientationLogic

        public PointOrientationData GetOrientationDataForPoint(Vector2 xzPoint, IEnumerable<IHexCell> candidateCells) {
            Profiler.BeginSample("PointOrientationLogic.GetOrientationDataForPoint()");

            var retval = new PointOrientationData();
            
            IHexCell bestCandidate = candidateCells.Where(BestCandidateFilter
                cell => (cell.AbsolutePositionXZ - xzPoint).sqrMagnitude <= SolidRadiusSq
            ).FirstOrDefault();

            if(bestCandidate != null) {
                retval.IsOnGrid = true;
                retval.Center   = bestCandidate;

                retval.CenterWeight = 1f;
            }

            Profiler.EndSample();

            return retval;
        }

        #endregion

        #endregion

    }

}
