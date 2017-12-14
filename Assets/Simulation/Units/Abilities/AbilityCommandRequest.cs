using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    [Serializable]
    public struct AbilityCommandRequest {

        #region instance fields and properties

        public AbilityCommandType CommandType;

        public List<string> ArgsToPass;

        #endregion

    }

}
