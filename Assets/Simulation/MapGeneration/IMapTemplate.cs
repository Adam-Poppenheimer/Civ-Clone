using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;

namespace Assets.Simulation.MapGeneration {

    public interface IMapTemplate {

        #region properties

        int CivCount { get; }

        IEnumerable<IHomelandTemplate> HomelandTemplates { get; }
        IEnumerable<IOceanTemplate>    OceanTemplates    { get; }

        IEnumerable<IRegionBiomeTemplate>    RegionBiomes     { get; }
        IEnumerable<IRegionTopologyTemplate> RegionTopologies { get; }

        int ContinentalLandPercentage { get; }

        int VoronoiPointCount { get; }

        int VoronoiPartitionIterations { get; }

        int MinStartingLocationDistance { get; }


        bool SeparateContinents { get; }

        float ContinentSeparationLineXMin { get; }
        float ContinentSeparationLineXMax { get; }


        int NeighborsInContinentWeight     { get; }
        int DistanceFromSeedCentroidWeight { get; }

        ReadOnlyCollection<IUnitTemplate> StartingUnits { get; }


        Texture2D PrecipitationTexture { get; }

        #endregion

    }

}
