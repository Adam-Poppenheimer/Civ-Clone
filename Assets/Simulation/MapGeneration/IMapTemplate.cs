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

        string name { get; }

        IEnumerable<IHomelandTemplate> HomelandTemplates { get; }
        IEnumerable<IOceanTemplate>    OceanTemplates    { get; }

        IEnumerable<IRegionBiomeTemplate>    RegionBiomes     { get; }
        IEnumerable<IRegionTopologyTemplate> RegionTopologies { get; }

        int VoronoiPointCount { get; }

        int VoronoiPartitionIterations { get; }

        int MinStartingLocationDistance { get; }

        int HomelandContiguousPercentage { get; }

        int HomelandExpansionMaxCentroidSeparation { get; }


        bool SeparateContinents { get; }

        float ContinentSeparationLineXMin { get; }
        float ContinentSeparationLineXMax { get; }

        


        int HardMapBorderX { get; }
        int HardMapBorderZ { get; }

        int SoftMapBorderX { get; }
        int SoftMapBorderZ { get; }

        int SoftBorderAvoidanceWeight { get; }


        int NeighborsInContinentWeight     { get; }
        int DistanceFromSeedCentroidWeight { get; }
        int DistanceFromMapCenterWeight    { get; }

        ReadOnlyCollection<IUnitTemplate> StartingUnits { get; }


        Texture2D PrecipitationTexture { get; }

        #endregion

    }

}
