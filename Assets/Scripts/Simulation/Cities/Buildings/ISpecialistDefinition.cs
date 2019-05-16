using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    public interface ISpecialistDefinition {

        #region properties

        string name { get; }

        Sprite Icon { get; }

        YieldSummary Yield { get; }

        #endregion

    }

}
