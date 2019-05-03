using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MountainHeightmapLogic : IMountainHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig         RenderConfig;
        private IHillsHeightmapLogic     HillsHeightmapLogic;
        private IFlatlandsHeightmapLogic FlatlandsHeightmapLogic;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapLogic(
            IMapRenderConfig renderConfig, IHillsHeightmapLogic hillsHeightmapLogic,
            IFlatlandsHeightmapLogic flatlandsHeightmapLogic
        ) {
            RenderConfig            = renderConfig;
            HillsHeightmapLogic     = hillsHeightmapLogic;
            FlatlandsHeightmapLogic = flatlandsHeightmapLogic;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapLogic

        
        public float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant, float elevationDuck) {
            Profiler.BeginSample("MountainHeightmapLogic.GetHeightForPoint()");

            float edgeWeight = Mathf.Clamp01((xzPoint - cell.AbsolutePositionXZ).magnitude / RenderConfig.InnerRadius);
            float peakWeight = 1f - edgeWeight;

            float edgeHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant, elevationDuck);

            float mountainHeight = edgeWeight * edgeHeight + peakWeight * RenderConfig.MountainPeakElevation;

            float flatlandHeight = FlatlandsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

            var retval = Mathf.Lerp(mountainHeight, flatlandHeight, elevationDuck);

            Profiler.EndSample();

            return retval;
        }

        #endregion

        #endregion
        
    }

}
