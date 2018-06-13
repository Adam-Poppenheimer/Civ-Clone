using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.HexMap {

    public class ImprovementFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig            Config;
        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        public ImprovementFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config, IImprovementLocationCanon improvementLocationCanon
        ) : base(noiseGenerator, featureContainer) {

            Config                   = config;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(
            IHexCell cell, Vector3 location, int locationIndex, HexHash hash
        ) {
            var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(improvementAtLocation == null || (
                hash.A >= Config.ImprovementAppearanceChance && locationIndex % Config.GuaranteedImprovementModulo != 0
            )) {
                return false;
            }

            AddFeature(improvementAtLocation.Template.AppearancePrefab, location, hash);

            return true;
        }

        #endregion

        #endregion

    }

}
