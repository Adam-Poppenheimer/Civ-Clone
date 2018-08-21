using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.MapGeneration {

    public interface IMapTemplate {

        #region properties

        int CivCount { get; }

        IEnumerable<ICivHomelandTemplate>     HomelandTemplates { get; }
        IEnumerable<IOceanTemplate> OceanTemplates    { get; }

        int ContinentalLandPercentage { get; }

        int VoronoiPointCount { get; }

        int VoronoiPartitionIterations { get; }

        int MinStartingLocationDistance { get; }


        int NeighborsInContinentWeight     { get; }
        int DistanceFromSeedCentroidWeight { get; }

        ReadOnlyCollection<IUnitTemplate> StartingUnits { get; }

        #endregion

    }

}
