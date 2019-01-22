using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnitUpgradeLine {

        #region properties

        ReadOnlyCollection<IUnitTemplate> Units { get; }

        #endregion

    }

}
