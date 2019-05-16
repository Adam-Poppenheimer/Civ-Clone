using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    [Serializable]
    public class AbilityCommandRequest {

        #region instance fields and properties

        public AbilityCommandType Type;

        public List<string> ArgsToPass;

        #endregion

    }

}
