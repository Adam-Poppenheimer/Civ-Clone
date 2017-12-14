using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Improvements {

    public interface IImprovement {

        #region properties

        bool IsComplete { get; }

        float WorkInvested { get; set; }

        IImprovementTemplate Template { get; }

        Transform transform { get; }

        #endregion

    }

}
