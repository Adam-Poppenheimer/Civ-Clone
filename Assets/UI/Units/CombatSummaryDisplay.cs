using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Cities;

namespace Assets.UI.Units {

    public class CombatSummaryDisplay : MonoBehaviour {

        #region instance fields and properties

        public IUnit AttackingUnit { get; set; }

        public IUnit DefendingUnit { get; set; }

        #endregion

        #region instance methods

        #region Unity messages



        #endregion

        #endregion

    }

}
