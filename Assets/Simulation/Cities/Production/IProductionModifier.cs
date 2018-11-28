using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Production {

    public interface IProductionModifier {

        #region properties

        float Value { get; }

        #endregion

        #region methods

        bool DoesModifierApply(IProductionProject project, ICity city);

        #endregion

    }

}
