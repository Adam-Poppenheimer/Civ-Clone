using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    [CreateAssetMenu(menuName = "Civ Clone/Technology/Technology")]
    public class TechDefinition : ScriptableObject, ITechDefinition {

        #region instance fields and properties

        #region from ITechDefinition

        public int Cost {
            get { return _cost; }
        }
        [SerializeField] private int _cost = 1;

        public string Name {
            get { return name; }
        }

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public TechnologyEra Era {
            get { return _era; }
        }
        [SerializeField] private TechnologyEra _era = TechnologyEra.Ancient;

        public int TechTableRow {
            get { return _techTableRow; }
        }
        [SerializeField] private int _techTableRow = 0;

        public int TechTableColumn {
            get { return _techTableColumn; }
        }
        [SerializeField] private int _techTableColumn = 0;

        public IEnumerable<ITechDefinition> Prerequisites {
            get { return _prerequisites.Cast<ITechDefinition>(); }
        }
        [SerializeField] private List<TechDefinition> _prerequisites = null;

        public IEnumerable<IBuildingTemplate> BuildingsEnabled {
            get { return _buildingsEnabled.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _buildingsEnabled = null;

        public IEnumerable<IUnitTemplate> UnitsEnabled {
            get { return _unitsEnabled.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _unitsEnabled = null;

        public IEnumerable<IAbilityDefinition> AbilitiesEnabled {
            get { return _abilitiesEnabled.Cast<IAbilityDefinition>(); }
        }
        [SerializeField] private List<AbilityDefinition> _abilitiesEnabled = null;

        public IEnumerable<IImprovementTemplate> ImprovementsEnabled {
            get { return _improvementsEnabled.Cast<IImprovementTemplate>(); }
        }
        [SerializeField] private List<ImprovementTemplate> _improvementsEnabled = null;

        public IEnumerable<IImprovementModificationData> ImprovementYieldModifications {
            get { return _improvementYieldModifications.Cast<IImprovementModificationData>(); }
        }
        [SerializeField] private List<ImprovementModificationData> _improvementYieldModifications = null;

        public IEnumerable<IResourceDefinition> RevealedResources {
            get { return _revealedResources.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _revealedResources = null;

        public IEnumerable<IPolicyTreeDefinition> PolicyTreesEnabled {
            get { return _policyTreesEnabled.Cast<IPolicyTreeDefinition>(); }
        }
        [SerializeField] private List<PolicyTreeDefinition> _policyTreesEnabled = null;

        public int FreeGreatPeopleProvided {
            get { return _freeGreatPeopleProvided; }
        }
        [SerializeField] private int _freeGreatPeopleProvided = 0;

        #endregion

        #endregion
        
    }

}
