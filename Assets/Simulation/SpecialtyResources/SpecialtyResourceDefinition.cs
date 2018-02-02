using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.SpecialtyResources {

    [CreateAssetMenu(menuName = "Civ Clone/Specialty Resource")]
    public class SpecialtyResourceDefinition : ScriptableObject, ISpecialtyResourceDefinition {

        #region instance fields and properties

        #region from ISpecialtyResourceDefinition

        public ResourceSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private ResourceSummary _bonusYield;

        public SpecialtyResourceType Type {
            get { return _type; }
        }
        [SerializeField] private SpecialtyResourceType _type;

        public IImprovementTemplate Extractor {
            get { return _extractor; }
        }
        [SerializeField] private ImprovementTemplate _extractor;

        public GameObject AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private GameObject _appearancePrefab;

        #endregion

        #endregion
        
    }

}
