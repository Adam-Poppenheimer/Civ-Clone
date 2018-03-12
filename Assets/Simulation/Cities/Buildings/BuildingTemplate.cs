using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Assets.Simulation.SpecialtyResources;
using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingTemplate.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Building")]
    public class BuildingTemplate : ScriptableObject, IBuildingTemplate {

        #region instance fields and properties

        #region from IBuildingTemplate

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        /// <inheritdoc/>
        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost;

        /// <inheritdoc/>
        public int Maintenance {
            get { return _maintenance; }
        }
        [SerializeField] private int _maintenance;

        /// <inheritdoc/>
        public ReadOnlyCollection<ResourceSummary> SlotYields {
            get { return _slotYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _slotYields;

        /// <inheritdoc/>
        public ResourceSummary StaticYield {
            get { return _staticYield; }
        }
        [SerializeField] private ResourceSummary _staticYield;

        /// <inheritdoc/>
        public ResourceSummary CivilizationYieldModifier {
            get { return _civilizationYieldModifier; }
        }
        [SerializeField] private ResourceSummary _civilizationYieldModifier;

        /// <inheritdoc/>
        public ResourceSummary CityYieldModifier {
            get { return _cityYieldModifier; }
        }
        [SerializeField] private ResourceSummary _cityYieldModifier;

        public int Health {
            get { return _health; }
        }
        [SerializeField] private int _health;

        public int Happiness {
            get { return _happiness; }
        }
        [SerializeField] private int _happiness;

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return _requiredResources.Cast<ISpecialtyResourceDefinition>(); }
        }
        [SerializeField] private List<SpecialtyResourceDefinition> _requiredResources;

        public IEnumerable<IResourceYieldModificationData> ResourceYieldModifications {
            get { return _resourceYieldModifications.Cast<IResourceYieldModificationData>(); }
        }
        [SerializeField] private List<ResourceYieldModificationData> _resourceYieldModifications;

        #endregion

        #endregion
        
    }

}
