using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.GameMap {

    public class TileResourceConfig : ScriptableObject, ITileResourceConfig {

        #region instance fields and properties

        #region from ITileResourceConfig

        public ResourceSummary GrasslandsYield {
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

        #endregion

        #endregion

    }

}
