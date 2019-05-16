using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    [CreateAssetMenu(menuName = "Civ Clone/Cities/Specialist")]
    public class SpecialistDefinition : ScriptableObject, ISpecialistDefinition {

        #region instance fields and properties

        #region from ISpecialistDefinition

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public YieldSummary Yield {
            get { return _yield; }
        }
        [SerializeField] private YieldSummary _yield;

        #endregion

        #endregion

    }

}
