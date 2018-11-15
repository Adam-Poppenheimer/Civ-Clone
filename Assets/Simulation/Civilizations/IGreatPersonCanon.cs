using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public interface IGreatPersonCanon {

        #region methods

        float GetPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ);
        void  AddPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ, float pointsAdded);
        void  SetPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ, float newPoints);

        float GetPointsNeededForTypeForCiv(GreatPersonType type, ICivilization civ);

        int  GetPredecessorsOfTypeForCiv(GreatPersonType type, ICivilization civ);

        void Clear();
        void ClearCiv(ICivilization civ);

        #endregion

    }

}
