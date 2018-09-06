using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation Template")]
    public class MapTemplate : ScriptableObject, IMapTemplate {

        #region instance fields and properties

        #region from IMapGenerationTemplate

        public int CivCount {
            get { return _civCount; }
        }
        [SerializeField, Range(2, 10)] private int _civCount = 8;

        public IEnumerable<IHomelandTemplate> HomelandTemplates {
            get { return _homelandTemplates.Cast<IHomelandTemplate>(); }
        }
        [SerializeField] private List<HomelandTemplate> _homelandTemplates;

        public IEnumerable<IOceanTemplate> OceanTemplates {
            get { return _oceanTemplates.Cast<IOceanTemplate>(); }
        }
        [SerializeField] private List<OceanTemplate> _oceanTemplates;




        public IEnumerable<IRegionBiomeTemplate> RegionBiomes {
            get { return _regionBiomes.Cast<IRegionBiomeTemplate>(); }
        }
        [SerializeField] private List<RegionBiomeTemplate> _regionBiomes;

        public IEnumerable<IRegionTopologyTemplate> RegionTopologies {
            get { return _regionTopologies.Cast<IRegionTopologyTemplate>(); }
        }
        [SerializeField] private List<RegionTopologyTemplate> _regionTopologies;



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

        public ReadOnlyCollection<IUnitTemplate> StartingUnits {
            get {
                if(_startingUnitsCast == null) {
                    _startingUnitsCast = _startingUnits.Cast<IUnitTemplate>().ToList();
                }

                return _startingUnitsCast.AsReadOnly();
            }
        }
        [NonSerialized]  private List<IUnitTemplate> _startingUnitsCast;
        [SerializeField] private List<UnitTemplate> _startingUnits;

        public Texture2D PrecipitationTexture {
            get { return _precipitationTexture; }
        }
        [SerializeField] private Texture2D _precipitationTexture;

        #endregion

        #endregion

    }

}
