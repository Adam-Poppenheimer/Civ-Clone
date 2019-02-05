using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class CityFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig                           Config;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CityFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config, IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ) : base(noiseGenerator, featureContainer) {

            Config            = config;
            CityLocationCanon = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(IHexCell cell, Vector3 location, int locationIndex, HexHash hash) {
            var cityOnCell = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(cityOnCell == null || (hash.A >= Config.BuildingAppearanceChance && locationIndex % Config.GuaranteedBuildingModulo != 0)) {
                return false;
            }

            int buildingIndex = (int)(hash.C * Config.BuildingPrefabs.Count);
            AddFeature(Config.BuildingPrefabs[buildingIndex], location, hash);

            return true;
        }

        #endregion

        #endregion

    }

}
