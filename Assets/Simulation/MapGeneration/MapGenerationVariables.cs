﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerationVariables : IMapGenerationVariables {

        #region instance fields and properties

        #region from IMapGenerationVariables

        public int ChunkCountX { get; set; }
        public int ChunkCountZ { get; set; }

        public int CivCount { get; set; }

        public int ContinentalLandPercentage { get; set; }

        #endregion

        #endregion

        #region constructors

        public MapGenerationVariables() {

        }

        #endregion

    }

}
