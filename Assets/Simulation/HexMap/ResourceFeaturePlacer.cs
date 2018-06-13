using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.HexMap {

    public class ResourceFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig                                   Config;
        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;

        #endregion

        #region constructors

        public ResourceFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config, IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon
        ) : base(noiseGenerator, featureContainer) {

            Config                    = config;
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(
            IHexCell cell, Vector3 location, int locationIndex, HexHash hash
        ) {
            var nodeOnCell = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(nodeOnCell == null || (hash.A >= Config.ResourceAppearanceChance && locationIndex % Config.GuaranteedResourceModulo != 0)) {
                return false;
            }

            AddFeature(nodeOnCell.Resource.AppearancePrefab, location, hash);
            return true;
        }

        #endregion

        #endregion

    }

}
