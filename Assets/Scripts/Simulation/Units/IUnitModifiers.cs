using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnitModifiers {

        #region properties

        IUnitModifier<float> ExperienceGain { get; }

        #endregion

    }

}
