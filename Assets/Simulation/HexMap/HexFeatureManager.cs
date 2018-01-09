﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexFeatureManager : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Transform FeaturePrefab;

        private INoiseGenerator NoiseGenerator;

        private Transform Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(INoiseGenerator noiseGenerator) {
            NoiseGenerator = noiseGenerator;
        }

        public void Clear() {
            if(Container != null) {
                Destroy(Container.gameObject);
            }

            Container = new GameObject("Features Container").transform;
            Container.SetParent(transform, false);
        }

        public void Apply() { }

        public void AddFeature(Vector3 position) {
            HexHash hash = NoiseGenerator.SampleHashGrid(position);
            if(hash.A >= 0.5f) {
                return;
            }

            Transform instance = Instantiate(FeaturePrefab);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = NoiseGenerator.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.B, 0f);
            instance.SetParent(Container, false);
        }

        #endregion

    }

}