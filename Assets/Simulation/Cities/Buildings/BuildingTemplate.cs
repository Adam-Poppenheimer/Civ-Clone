using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingTemplate.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Building")]
    public class BuildingTemplate : ScriptableObject, IBuildingTemplate {

        #region instance fields and properties

        #region from IBuildingTemplate

        /// <inheritdoc/>
        public int Cost {
            get { return _cost; }
        }
        [SerializeField] private int _cost;

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

        #endregion

        #endregion
        
    }

}
