using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class RuinsFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig Config;

        #endregion

        #region constructors

        [Inject]
        public RuinsFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config
        ) : base(noiseGenerator, featureContainer) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(IHexCell cell, Vector3 location, int locationIndex, HexHash hash) {
            if( cell.Feature != CellFeature.CityRuins ||
                (hash.A >= Config.RuinsAppearanceChance) && locationIndex % Config.GuaranteedRuinsModulo != 0
            ) {
                return false;
            }

            int ruinIndex = (int)(hash.C * Config.RuinsPrefabs.Count);
            AddFeature(Config.RuinsPrefabs[ruinIndex], location, hash);

            return true;
        }

        #endregion

        #endregion

    }

}
