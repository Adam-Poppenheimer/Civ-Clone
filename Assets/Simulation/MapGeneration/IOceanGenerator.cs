﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanGenerator {

        #region methods

        void GenerateOcean(
            MapRegion ocean, IOceanGenerationTemplate template, IEnumerable<IHexCell> continentCells
        );

        #endregion

    }

}
