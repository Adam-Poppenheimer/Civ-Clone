using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.AI {

    public interface IInfluenceMapLogic {

        #region methods

        void ClearMaps (InfluenceMaps maps);
        void AssignMaps(InfluenceMaps maps, ICivilization targetCiv);

        #endregion

    }

}
