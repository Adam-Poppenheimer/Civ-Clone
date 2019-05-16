using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class EncampmentFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig           Config;
        private IEncampmentLocationCanon EncampmentLocationCanon;

        #endregion

        #region constructors

        public EncampmentFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config, IEncampmentLocationCanon encampmentLocationCanon

        ) : base(noiseGenerator, featureContainer) {
            Config                  = config;
            EncampmentLocationCanon = encampmentLocationCanon;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(IHexCell cell, Vector3 location, int locationIndex, HexHash hash) {
            var encampmentOnCell = EncampmentLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(encampmentOnCell == null || (hash.A >= Config.EncampmentAppearanceChance && locationIndex % Config.GuaranteedEncampmentModulo != 0)) {
                return false;
            }

            int encampmentIndex = (int)(hash.C * Config.EncampmentPrefabs.Count);
            AddFeature(Config.EncampmentPrefabs[encampmentIndex], location, hash);

            return true;
        }

        #endregion

        #endregion

    }

}
