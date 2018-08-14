﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationTemplate {

        #region properties

        int CivCount { get; }

        IEnumerable<IRegionGenerationTemplate> CivRegionTemplates { get; }
        IEnumerable<IOceanGenerationTemplate>  OceanTemplates     { get; }

        int ContinentalLandPercentage { get; }

        int VoronoiPointCount { get; }

        int VoronoiPartitionIterations { get; }

        int MinStartingLocationDistance { get; }


        int NeighborsInContinentWeight     { get; }
        int DistanceFromSeedCentroidWeight { get; }

        #endregion

    }

}
