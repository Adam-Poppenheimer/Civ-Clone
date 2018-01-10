using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Unit Template")]
    public class UnitTemplate : ScriptableObject, IUnitTemplate {

        #region instance fields and properties

        #region from IUnitTemplate

        public int MaxMovement {
            get { return _maxMovement; }
        }
        [SerializeField] private int _maxMovement;

        public string Name {
            get { return _name; }
        }
        [SerializeField] private string _name;

        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost;

        public UnitType Type {
            get { return _type; }
        }
        [SerializeField] private UnitType _type;

        public bool IsAquatic {
            get { return Type == UnitType.WaterMilitary || Type == UnitType.WaterCivilian; }
        }

        public IEnumerable<IUnitAbilityDefinition> Abilities {
            get { return _abilities.Cast<IUnitAbilityDefinition>(); }
        }
        [SerializeField] private List<UnitAbilityDefinition> _abilities;

        #endregion

        #endregion
        
    }

}
