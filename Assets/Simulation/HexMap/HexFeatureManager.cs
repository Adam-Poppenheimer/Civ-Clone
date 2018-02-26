using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexFeatureManager : MonoBehaviour {

        #region instance fields and properties

        private Transform Container;

        private DictionaryOfLists<IHexCell, Vector3> FeaturePositionsForCell =
            new DictionaryOfLists<IHexCell, Vector3>();




        private INoiseGenerator NoiseGenerator;

        private IFeatureConfig Config;

        private ICityFactory CityFactory;

        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            INoiseGenerator noiseGenerator, IFeatureConfig config, ICityFactory cityFactory,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon
        ){
            NoiseGenerator            = noiseGenerator;
            Config                    = config;
            CityFactory               = cityFactory;
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
        }

        public void Clear() {
            if(Container != null) {
                Destroy(Container.gameObject);
            }

            Container = new GameObject("Features Container").transform;
            Container.SetParent(transform, false);
        }

        public void Apply() {
            foreach(var cellWithFeatures in FeaturePositionsForCell.Keys) {
                ApplyFeaturesToCell(cellWithFeatures, FeaturePositionsForCell[cellWithFeatures]);
            }

            FeaturePositionsForCell.Clear();
        }

        public void FlagLocationForFeatures(Vector3 location, IHexCell cell) {
            FeaturePositionsForCell[cell].Add(location);
        }

        private void ApplyFeaturesToCell(IHexCell cell, List<Vector3> locations) {
            var cityOnCell = CityFactory.AllCities.Where(city => city.Location == cell).FirstOrDefault();
            var nodeOnCell = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(cityOnCell != null) {
                ApplyCityFeaturesToCell(cell, locations);

            }else if(nodeOnCell != null) {
                ApplyResourceNodeToCell(cell, locations, nodeOnCell);

            }else if(cell.Feature == TerrainFeature.Forest) {
                ApplyForestToCell(cell, locations);

            }
        }

        private void ApplyCityFeaturesToCell(IHexCell cell, List<Vector3> locations) {
            foreach(var location in locations) {
                ApplyBuildingToLocation(location, location == locations.First());
            }
        }

        private void ApplyResourceNodeToCell(IHexCell cell, List<Vector3> locations, IResourceNode node) {
            foreach(var location in locations) {
                if(!ApplyResourceNodeToLocation(location, node, location == locations.First())) {
                    
                    if(cell.Feature == TerrainFeature.Forest) {
                        ApplyForestToLocation(location);
                    }
                }
            }
        }

        private void ApplyForestToCell(IHexCell cell, List<Vector3> locations) {
            foreach(var location in locations) {
                ApplyForestToLocation(location, location == locations.First());
            }
        }


        private bool ApplyBuildingToLocation(Vector3 location, bool forcePopulate = false) {
            HexHash hash = NoiseGenerator.SampleHashGrid(location);
            if(!forcePopulate && hash.A >= Config.BuildingAppearanceChance) {
                return false;
            }

            int buildingIndex = (int)(hash.C * Config.BuildingPrefabs.Count);
            AddFeature(Config.BuildingPrefabs[buildingIndex], location, hash);

            return true;
        }

        private bool ApplyForestToLocation(Vector3 location, bool forcePopulate = false) {
            HexHash hash = NoiseGenerator.SampleHashGrid(location);
            if(!forcePopulate && hash.A >= Config.TreeAppearanceChance) {
                return false;
            }

            int treeIndex = (int)(hash.C * Config.TreePrefabs.Count);
            AddFeature(Config.TreePrefabs[treeIndex], location, hash);

            return true;
        }

        private bool ApplyResourceNodeToLocation(Vector3 location, IResourceNode node, bool forcePopulate = false) {
            HexHash hash = NoiseGenerator.SampleHashGrid(location);
            if(!forcePopulate && hash.A >= Config.ResourceAppearanceChance) {
                return false;
            }

            AddFeature(node.Resource.AppearancePrefab, location, hash);
            return true;
        }


        private void AddFeature(Transform prefab, Vector3 location, HexHash hash) {
            Transform instance = Instantiate(prefab);
            instance.localPosition = NoiseGenerator.Perturb(location);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.B, 0f);
            instance.SetParent(Container, false);
        }

        #endregion

    }

}
