﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionGenerator {

        #region methods

        void GenerateTopologyAndEcology(
            MapRegion region, IRegionGenerationTemplate template
        );

        void DistributeYieldAndResources(
            MapRegion region, IRegionGenerationTemplate template
        );

        #endregion

    }

}
