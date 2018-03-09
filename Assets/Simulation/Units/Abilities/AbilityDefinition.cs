using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    [CreateAssetMenu(menuName = "Civ Clone/Ability")]
    public class AbilityDefinition : ScriptableObject, IAbilityDefinition {

        #region instance fields and properties

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public bool RequiresMovement {
            get { return _requiresMovement; }
        }
        [SerializeField] private bool _requiresMovement;

        public bool ConsumesMovement {
            get { return _consumesMovement; }
        }
        [SerializeField] private bool _consumesMovement;

        public bool DestroysUnit {
            get { return _destroysUnit; }
        }
        [SerializeField] private bool _destroysUnit;

        public IEnumerable<AbilityCommandRequest> CommandRequests {
            get { return _commandRequests; }
        }
        [SerializeField] private List<AbilityCommandRequest> _commandRequests;

        #endregion

    }

}
