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

        public ResourceSummary BonusYieldBase {
            get { return _bonusYieldBase; }
        }
        [SerializeField] private ResourceSummary _bonusYieldBase;

        public ResourceSummary BonusYieldWhenImproved {
            get { return _bonusYieldWhenImproved; }
        }
        [SerializeField] private ResourceSummary _bonusYieldWhenImproved;

        public SpecialtyResourceType Type {
            get { return _type; }
        }
        [SerializeField] private SpecialtyResourceType _type;

        public IImprovementTemplate Extractor {
            get { return _extractor; }
        }
        [SerializeField] private ImprovementTemplate _extractor;

        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        #endregion

        #endregion
        
    }

}
