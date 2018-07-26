using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    [CreateAssetMenu(menuName = "Civ Clone/Specialty Resource")]
    public class ResourceDefinition : ScriptableObject, IResourceDefinition {

        #region instance fields and properties

        #region from ISpecialtyResourceDefinition

        public YieldSummary BonusYieldBase {
            get { return _bonusYieldBase; }
        }
        [SerializeField] private YieldSummary _bonusYieldBase;

        public YieldSummary BonusYieldWhenImproved {
            get { return _bonusYieldWhenImproved; }
        }
        [SerializeField] private YieldSummary _bonusYieldWhenImproved;

        public ResourceType Type {
            get { return _type; }
        }
        [SerializeField] private ResourceType _type;

        public IImprovementTemplate Extractor {
            get { return _extractor; }
        }
        [SerializeField] private ImprovementTemplate _extractor;

        public IEnumerable<CellTerrain> PermittedTerrains {
            get { return _permittedTerrains; }
        }
        [SerializeField] private List<CellTerrain> _permittedTerrains;

        public IEnumerable<CellShape> PermittedShapes {
            get { return _permittedShapes; }
        }
        [SerializeField] private List<CellShape> _permittedShapes;

        public IEnumerable<CellVegetation> PermittedVegetations {
            get { return _permittedVegetations; }
        }
        [SerializeField] private List<CellVegetation> _permittedVegetations;

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
