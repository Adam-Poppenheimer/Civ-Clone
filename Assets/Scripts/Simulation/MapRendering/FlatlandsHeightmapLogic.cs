﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

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

        public float GetHeightForPoint(
            Vector2 xzPoint, AsyncTextureUnsafe<Color32> noiseTexture
        ) {
            float noise = NoiseGenerator.SampleNoise(
                xzPoint, noiseTexture, RenderConfig.FlatlandsElevationNoiseStrength, NoiseType.ZeroToOne
            ).x;

            return RenderConfig.FlatlandsBaseElevation + noise;
        }

        #endregion

        #endregion

    }

}
