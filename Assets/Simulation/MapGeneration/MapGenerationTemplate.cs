using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation Template")]
    public class MapGenerationTemplate : ScriptableObject, IMapGenerationTemplate {

        #region instance fields and properties

        #region from IMapGenerationTemplate

        public int CivCount {
            get { return _civCount; }
        }
        [SerializeField, Range(2, 10)] private int _civCount = 8;

        public IEnumerable<IRegionGenerationTemplate> CivRegionTemplates {
            get { return _civRegionTemplates.Cast<IRegionGenerationTemplate>(); }
        }
        [SerializeField] private List<RegionGenerationTemplate> _civRegionTemplates;

        public IEnumerable<IOceanGenerationTemplate> OceanTemplates {
            get { return _oceanTemplates.Cast<IOceanGenerationTemplate>(); }
        }
        [SerializeField] private List<OceanGenerationTemplate> _oceanTemplates;

        public int ContinentalLandPercentage {
            get { return _continentalLandPercentage; }
        }
        [SerializeField, Range(1, 100)] private int _continentalLandPercentage;

        public int VoronoiPointCount {
            get { return _voronoiPointCount; }
        }
        [SerializeField, Range(50, 1000)] private int _voronoiPointCount;

        public int VoronoiPartitionIterations {
            get { return _voronoiPartitionIterations; }
        }
        [SerializeField, Range(1, 20)] private int _voronoiPartitionIterations;

        public int MinStartingLocationDistance {
            get { return _minStartingLocationDistance; }
        }
        [SerializeField, Range(1, 15)] private int _minStartingLocationDistance;

        public int NeighborsInContinentWeight {
            get { return _neighborsInContinentWeight; }
        }
        [SerializeField] private int _neighborsInContinentWeight;

        public int DistanceFromSeedCentroidWeight {
            get { return _distanceFromSeedCentroidWeight; }
        }
        [SerializeField] private int _distanceFromSeedCentroidWeight;

        #endregion

        #endregion
        
    }

}
