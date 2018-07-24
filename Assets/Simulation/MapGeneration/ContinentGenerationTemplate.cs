using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Continent Generation Template")]
    public class ContinentGenerationTemplate : ScriptableObject, IContinentGenerationTemplate {

        #region instance fields and properties

        #region from IContinentGenerationTemplate

        public int LandPercentage {
            get { return _landPercentage; }
        }
        [SerializeField, Range(1, 100)] private int _landPercentage;

        public int SoftBorderX {
            get { return _softBorderX; }
        }
        [SerializeField] private int _softBorderX;

        public int SoftBorderZ {
            get { return _softBorderZ; }
        }
        [SerializeField] private int _softBorderZ;

        public int StartingAreaCount {
            get { return _startingAreaCount; }
        }
        [SerializeField, Range(1, 8)] private int _startingAreaCount = 2;

        public IEnumerable<IRegionGenerationTemplate> StartingLocationTemplates {
            get { return _startingLocationTemplates.Cast<IRegionGenerationTemplate>(); }
        }
        [SerializeField] private List<RegionGenerationTemplate> _startingLocationTemplates;

        public IEnumerable<IRegionGenerationTemplate> BoundaryTemplates {
            get { return _boundaryTemplates.Cast<IRegionGenerationTemplate>(); }
        }
        [SerializeField] private List<RegionGenerationTemplate> _boundaryTemplates;

        #endregion

        #endregion

    }

}
