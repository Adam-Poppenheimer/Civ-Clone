using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Technology {

    [CreateAssetMenu(menuName = "Civ Clone/Technology")]
    public class TechDefinition : ScriptableObject, ITechDefinition {

        #region instance fields and properties

        #region from ITechDefinition

        public int Cost {
            get { return _cost; }
        }
        [SerializeField] private int _cost;

        public string Name {
            get { return name; }
        }

        public IEnumerable<ITechDefinition> Prerequisites {
            get { return _prerequisites.Cast<ITechDefinition>(); }
        }
        [SerializeField] private List<TechDefinition> _prerequisites;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public int TechTableRow {
            get { return _techTableRow; }
        }
        [SerializeField] private int _techTableRow;

        public int TechTableColumn {
            get { return _techTableColumn; }
        }
        [SerializeField] private int _techTableColumn;

        public IEnumerable<IBuildingTemplate> BuildingsEnabled {
            get { return _buildingsEnabled.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _buildingsEnabled;

        public IEnumerable<IUnitTemplate> UnitsEnabled {
            get { return _unitsEnabled.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _unitsEnabled;

        public IEnumerable<IAbilityDefinition> AbilitiesEnabled {
            get { return _abilitiesEnabled.Cast<IAbilityDefinition>(); }
        }
        [SerializeField] private List<AbilityDefinition> _abilitiesEnabled;

        public IEnumerable<IImprovementModificationData> ImprovementYieldModifications {
            get { return _improvementYieldModifications.Cast<IImprovementModificationData>(); }
        }
        [SerializeField] private List<ImprovementModificationData> _improvementYieldModifications;

        public IEnumerable<ISpecialtyResourceDefinition> RevealedResources {
            get { return _revealedResources.Cast<ISpecialtyResourceDefinition>(); }
        }
        [SerializeField] private List<SpecialtyResourceDefinition> _revealedResources;

        #endregion

        #endregion
        
    }

}
