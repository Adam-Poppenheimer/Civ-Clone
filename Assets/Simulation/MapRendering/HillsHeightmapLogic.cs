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

    public class HillsHeightmapLogic : IHillsHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig         RenderConfig;
        private INoiseGenerator          NoiseGenerator;
        private IFlatlandsHeightmapLogic FlatlandsHeightmapLogic;

        #endregion

        #region constructors

        [Inject]
        public HillsHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IFlatlandsHeightmapLogic flatlandsHeightmapLogic
        ) {
            RenderConfig            = renderConfig;
            NoiseGenerator          = noiseGenerator;
            FlatlandsHeightmapLogic = flatlandsHeightmapLogic;
        }

        #endregion

        #region instance methods

        #region from IHillsHeightmapLogic

        public float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant, float elevationDuck) {
            Profiler.BeginSample("HillHeightmapLogic.GetHeightForPoint()");

            float hillNoise = NoiseGenerator.SampleNoise(
                xzPoint, RenderConfig.HillsElevationNoiseSource, RenderConfig.HillsElevationNoiseStrength,
                NoiseType.ZeroToOne
            ).x;

            float hillsHeight = RenderConfig.HillsBaseElevation + hillNoise;

            float flatlandsHeight = FlatlandsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

            var retval = Mathf.Lerp(hillsHeight, flatlandsHeight, elevationDuck);

            Profiler.EndSample();

            return retval;
        }

        #endregion

        #endregion
        
    }

}
