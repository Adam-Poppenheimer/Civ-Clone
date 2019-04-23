using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class FlatlandsHeightmapLogic : IFlatlandsHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private INoiseGenerator  NoiseGenerator;

        #endregion

        #region constructors

        public FlatlandsHeightmapLogic(IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator) {
            RenderConfig   = renderConfig;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IFlatlandsHeightmapLogic

        public float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            Profiler.BeginSample("FlatlandsHeightmapLogic.GetHeightForPoint()");

            Vector4 noise = NoiseGenerator.SampleNoise(
                xzPoint, RenderConfig.FlatlandsElevationNoiseSource, RenderConfig.FlatlandsElevationNoiseStrength,
                NoiseType.ZeroToOne
            );

            Profiler.EndSample();
            return RenderConfig.FlatlandsBaseElevation + noise.x;
        }

        #endregion

        #endregion

    }

}
