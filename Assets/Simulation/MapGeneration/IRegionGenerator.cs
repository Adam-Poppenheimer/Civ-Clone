﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionGenerator {

        #region methods

        void GenerateRegion(MapRegion region, IRegionGenerationTemplate template);

        #endregion

    }

}
