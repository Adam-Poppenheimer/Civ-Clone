﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets.Simulation.HexMap {

    public class TreeFeaturePlacer : FeaturePlacerBase {

        #region instance fields and properties

        private IFeatureConfig Config;

        #endregion

        #region constructors

        public TreeFeaturePlacer(
            INoiseGenerator noiseGenerator, [Inject(Id = "Feature Container")] Transform featureContainer,
            IFeatureConfig config
        ) : base(noiseGenerator, featureContainer) {

            Config = config;
        }

        #endregion

        #region instance methods

        #region from FeaturePlacerBase

        public override bool TryPlaceFeatureAtLocation(
            IHexCell cell, Vector3 location, int locationIndex, HexHash hash
        ) {
            if( (cell.Feature != TerrainFeature.Forest && cell.Feature != TerrainFeature.Jungle) ||
                (hash.A >= Config.TreeAppearanceChance && locationIndex % Config.GuaranteedTreeModulo != 0)
            ){
                return false;
            }

            if(cell.Feature == TerrainFeature.Forest) {
                int treeIndex = (int)(hash.C * Config.ForestTreePrefabs.Count);

                AddFeature(Config.ForestTreePrefabs[treeIndex], location, hash);

            }else {
                int treeIndex = (int)(hash.C * Config.JungleTreePrefabs.Count);

                AddFeature(Config.JungleTreePrefabs[treeIndex], location, hash);
            }

            return true;
        }

        #endregion

        #endregion

    }

}
