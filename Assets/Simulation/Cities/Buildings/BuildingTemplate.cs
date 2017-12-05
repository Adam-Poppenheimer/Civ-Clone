using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    [CreateAssetMenu(menuName = "Civ Clone/Building Template")]
    public class BuildingTemplate : ScriptableObject, IBuildingTemplate {

        #region instance fields and properties

        #region from IBuildingTemplate

        public int Cost {
            get { return _cost; }
        }
        [SerializeField] private int _cost;

        public int Maintenance {
            get { return _maintenance; }
        }
        [SerializeField] private int _maintenance;

        public ReadOnlyCollection<ResourceSummary> SlotYields {
            get { return _slotYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _slotYields;

        public ResourceSummary StaticYield {
            get { return _staticYield; }
        }
        [SerializeField] private ResourceSummary _staticYield;

        public ResourceSummary CivilizationYieldModifier {
            get { return _civilizationYieldModifier; }
        }
        [SerializeField] private ResourceSummary _civilizationYieldModifier;

        public ResourceSummary CityYieldModifier {
            get { return _cityYieldModifier; }
        }
        [SerializeField] private ResourceSummary _cityYieldModifier;

        #endregion

        #endregion
        
    }

}
