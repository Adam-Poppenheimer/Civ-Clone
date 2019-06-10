using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public abstract class FeaturePlacerBase : IFeaturePlacer {

        #region instance fields and properties

        private INoiseGenerator NoiseGenerator;
        private Transform       FeatureContainer;

        #endregion

        #region constructors

        [Inject]
        public FeaturePlacerBase(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer
        ) {
            NoiseGenerator   = noiseGenerator;
            FeatureContainer = featureContainer;
        }

        #endregion

        #region instance methods

        public abstract bool TryPlaceFeatureAtLocation(
            IHexCell cell, Vector3 location, int locationIndex, HexHash hash
        );

        protected void AddFeature(Transform prefab, Vector3 location, HexHash hash) {
            Transform instance = GameObject.Instantiate(prefab, FeatureContainer);

            instance.position      = NoiseGenerator.Perturb(location);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.B, 0f);
        }

        #endregion

    }

}
