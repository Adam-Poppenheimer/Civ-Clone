using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.HexMap {

    public class HexFeatureManager : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Transform FeaturePrefab;

        [SerializeField] private List<Transform> TreePrefabs;

        [SerializeField] private List<Transform> BuildingPrefabs;

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

        public void AddFeature(Vector3 position, TerrainFeature featureType) {
            HexHash hash = NoiseGenerator.SampleHashGrid(position);
            if(hash.A >= 0.5f) {
                return;
            }

            Transform prefabToUse = GetFeaturePrefab(featureType, hash);

            Transform instance = Instantiate(prefabToUse);
            instance.localPosition = NoiseGenerator.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.B, 0f);
            instance.SetParent(Container, false);
        }

        public void AddCityFeature(ICity city, Vector3 position) {
            HexHash hash = NoiseGenerator.SampleHashGrid(position);

            Transform prefabToUse = GetCityPrefab(hash);

            Transform instance = Instantiate(prefabToUse);
            instance.localPosition = NoiseGenerator.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.B, 0f);
            instance.SetParent(Container, false);
        }

        private Transform GetFeaturePrefab(TerrainFeature featureType, HexHash hash) {
            if(featureType == TerrainFeature.Forest) {
                int treeIndex = (int)(hash.C * TreePrefabs.Count);
                return TreePrefabs[treeIndex];
            }else {
                return FeaturePrefab;
            }
        }

        private Transform GetCityPrefab(HexHash hash) {
            int buildingIndex = (int)(hash.C * BuildingPrefabs.Count);
            return BuildingPrefabs[buildingIndex];
        }

        #endregion

    }

}
