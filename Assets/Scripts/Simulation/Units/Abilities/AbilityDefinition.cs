using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    [CreateAssetMenu(menuName = "Civ Clone/Units/Ability")]
    public class AbilityDefinition : ScriptableObject, IAbilityDefinition {

        #region instance fields and properties

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description = null;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public bool RequiresMovement {
            get { return _requiresMovement; }
        }
        [SerializeField] private bool _requiresMovement = false;

        public bool ConsumesMovement {
            get { return _consumesMovement; }
        }
        [SerializeField] private bool _consumesMovement = false;

        public bool DestroysUnit {
            get { return _destroysUnit; }
        }
        [SerializeField] private bool _destroysUnit = false;

        public IEnumerable<AbilityCommandRequest> CommandRequests {
            get { return _commandRequests; }
        }
        [SerializeField] private List<AbilityCommandRequest> _commandRequests = null;

        #endregion

    }

}
