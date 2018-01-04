﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(fileName = "New Tile Display Config", menuName = "Civ Clone/Tile Display Config")]
    public class TileConfig : ScriptableObject, ITileConfig {

        #region instance fields and properties

        public ReadOnlyCollection<Color> ColorsOfTerrains {
            get { return _colorOfTerrains.AsReadOnly(); }
        }
        [SerializeField] private List<Color> _colorOfTerrains;

        public ReadOnlyCollection<int> ElevationsOfShapes {
            get { return _elevationOfShapes.AsReadOnly(); }
        }
        [SerializeField] private List<int> _elevationOfShapes;

        public ResourceSummary GrasslandYield {
            get { return _grasslandsYield; }
        }
        [SerializeField] private ResourceSummary _grasslandsYield;

        public ResourceSummary PlainsYield {
            get { return _plainsYield; }
        }
        [SerializeField] private ResourceSummary _plainsYield;

        public ResourceSummary DesertYield {
            get { return _desertYield; }
        }
        [SerializeField] private ResourceSummary _desertYield;

        public ResourceSummary ForestYield {
            get { return _forestYield; }
        }
        [SerializeField] private ResourceSummary _forestYield;

        public ResourceSummary HillsYield {
            get { return _hillsYield; }
        }
        [SerializeField] private ResourceSummary _hillsYield;

        public int GrasslandMoveCost {
            get { return _grasslandMoveCost; }
        }
        [SerializeField] private int _grasslandMoveCost;

        public int PlainsMoveCost {
            get { return _plainsMoveCost; }
        }
        [SerializeField] private int _plainsMoveCost;

        public int DesertMoveCost {
            get { return _desertMoveCost; }
        }
        [SerializeField] private int _desertMoveCost;

        public int HillsMoveCost {
            get { return _hillsMoveCost; }
        }
        [SerializeField] private int _hillsMoveCost;

        public int ForestMoveCost {
            get { return _forestMoveCost; }
        }
        [SerializeField] private int _forestMoveCost;

        public int ShallowWaterMoveCost {
            get { return _shallowWaterMoveCost; }
        }
        [SerializeField] private int _shallowWaterMoveCost;

        public int DeepWaterMoveCost {
            get { return _deepWaterMoveCost; }
        }
        [SerializeField] private int _deepWaterMoveCost;

        public ReadOnlyCollection<TerrainType> UnoccupiableTerrains {
            get { return _unoccupiableTerrains.AsReadOnly(); }
        }
        [SerializeField] private List<TerrainType> _unoccupiableTerrains;

        #endregion

    }

}
