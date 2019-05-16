using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.AI {

    public interface IInfluenceSource {

        #region methods

        void ApplyToMaps(InfluenceMaps maps, ICivilization targetCiv);

        #endregion

    }

}
