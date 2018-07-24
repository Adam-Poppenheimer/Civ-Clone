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

        public IEnumerable<MapSection> ContinentSections {
            get { return _continentSections; }
        }
        [SerializeField] private List<MapSection> _continentSections;

        public IEnumerable<IContinentGenerationTemplate> ContinentTemplates {
            get { return _continentTemplates.Cast<IContinentGenerationTemplate>(); }
        }
        [SerializeField] private List<ContinentGenerationTemplate> _continentTemplates;

        public IEnumerable<IOceanGenerationTemplate> OceanTemplates {
            get { return _oceanTemplates.Cast<IOceanGenerationTemplate>(); }
        }
        [SerializeField] private List<OceanGenerationTemplate> _oceanTemplates;

        #endregion

        #endregion
        
    }

}
