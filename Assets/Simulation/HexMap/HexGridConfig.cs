﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(fileName = "New Hex Grid Config", menuName = "Civ Clone/Hex Grid Config")]
    public class HexGridConfig : ScriptableObject, IHexGridConfig {

        #region instance fields and properties

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed;

        public ReadOnlyCollection<Color> ColorsOfTerrains {
            get { return _colorOfTerrains.AsReadOnly(); }
        }
        [SerializeField] private List<Color> _colorOfTerrains;

        public ReadOnlyCollection<ResourceSummary> TerrainYields {
            get { return _terrainYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _terrainYields;

        public ReadOnlyCollection<ResourceSummary> FeatureYields {
            get { return _featureYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _featureYields;

        public int BaseLandMoveCost {
            get { return _baseLandMoveCost; }
        }
        [SerializeField] private int _baseLandMoveCost;

        public ReadOnlyCollection<int> FeatureMoveCosts {
            get { return _featureMoveCosts.AsReadOnly(); }
        }
        [SerializeField] private List<int> _featureMoveCosts;

        public int WaterMoveCost {
            get { return _waterMoveCost; }
        }
        [SerializeField] private int _waterMoveCost;

        public int SlopeMoveCost {
            get { return _slopeMoveCost; }
        }
        [SerializeField] private int _slopeMoveCost;

        #endregion

    }

}
