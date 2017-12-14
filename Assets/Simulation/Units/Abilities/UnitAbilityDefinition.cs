using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    [CreateAssetMenu(menuName = "Civ Clone/Unit Ability Definition")]
    public class UnitAbilityDefinition : ScriptableObject, IUnitAbilityDefinition {

        #region instance fields and properties

        public IEnumerable<AbilityCommandRequest> CommandRequests {
            get { return _commandRequests; }
        }
        [SerializeField] private List<AbilityCommandRequest> _commandRequests;

        #endregion

    }

}
