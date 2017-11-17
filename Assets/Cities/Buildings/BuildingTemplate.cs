using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Assets.Cities;
using UnityEngine;

namespace Assets.Cities.Buildings {

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

        #endregion

        #endregion
        
    }

}
