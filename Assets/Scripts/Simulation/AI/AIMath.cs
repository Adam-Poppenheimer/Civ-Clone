using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.AI {

    public static class AIMath {

        #region static methods

        public static float NormalizedLogisticCurve(float sigmoidMid, float growthRate, float x) {
            return 1f / (1 + Mathf.Exp(-growthRate * (x - sigmoidMid)));
        }

        #endregion

    }

}
