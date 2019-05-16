using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class HexFeatureManager : IHexFeatureManager {

        #region instance fields and properties

        private DictionaryOfLists<IHexCell, Vector3> FeaturePositionsForCell =
            new DictionaryOfLists<IHexCell, Vector3>();




        private INoiseGenerator       NoiseGenerator;
        private IFeatureLocationLogic FeatureLocationLogic;
        private List<IFeaturePlacer>  FeaturePlacers;
        private Transform             FeatureContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            INoiseGenerator noiseGenerator, IFeatureLocationLogic featureLocationLogic,
            List<IFeaturePlacer> featurePlacers,
            [InjectOptional(Id = "Feature Container")] Transform featureContainer
        ){
            NoiseGenerator       = noiseGenerator;
            FeatureLocationLogic = featureLocationLogic;
            FeaturePlacers       = featurePlacers;
            FeatureContainer     = featureContainer;
        }

        #region from IHexFeatureManager

        public void Clear() {
            if(FeatureContainer == null) {
                return;
            }

            for(int i = FeatureContainer.childCount - 1; i >= 0; --i) {
                if(Application.isPlaying) {
                    GameObject.Destroy(FeatureContainer.GetChild(i).gameObject);
                }else {
                    GameObject.DestroyImmediate(FeatureContainer.GetChild(i).gameObject);
                }                
            }
        }

        public void Apply() {
            foreach(var cellWithFeatures in FeaturePositionsForCell.Keys) {
                ApplyFeaturesToCell(cellWithFeatures, FeaturePositionsForCell[cellWithFeatures]);
            }

            FeaturePositionsForCell.Clear();
        }



        public void AddFeatureLocationsForCell(IHexCell cell) {
            var listOfLocations = new List<Vector3>();

            listOfLocations.AddRange(FeatureLocationLogic.GetCenterFeaturePoints(cell));

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                listOfLocations.AddRange(FeatureLocationLogic.GetDirectionalFeaturePoints(cell, direction));                
            }

            FeaturePositionsForCell[cell] = listOfLocations;
        }

        public IEnumerable<Vector3> GetFeatureLocationsForCell(IHexCell cell) {
            return FeaturePositionsForCell[cell];
        }

        #endregion

        private void ApplyFeaturesToCell(IHexCell cell, List<Vector3> locations) {
            for(int i = 0; i < locations.Count; i++) {
                var location = locations[i];

                var locationHash = NoiseGenerator.SampleHashGrid(location);

                foreach(var featurePlacer in FeaturePlacers) {
                    if(featurePlacer.TryPlaceFeatureAtLocation(cell, location, i, locationHash)) {
                        break;
                    }
                }
            }
        }

        #endregion

    }

}
