using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Units/Upgrade Line")]
    public class UnitUpgradeLine : ScriptableObject, IUnitUpgradeLine {

        #region instance fields and properties

        #region from IUnitUpgradeLine

        public ReadOnlyCollection<IUnitTemplate> Units {
            get {
                if(_castUnits == null) {
                    _castUnits = _units.Cast<IUnitTemplate>().ToList();
                }

                return _castUnits.AsReadOnly();
            }
        }
        private List<IUnitTemplate> _castUnits;
        [SerializeField] private List<UnitTemplate> _units;

        #endregion

        #endregion
        
    }

}
