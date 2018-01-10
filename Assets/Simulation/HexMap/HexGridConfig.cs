using System;
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
