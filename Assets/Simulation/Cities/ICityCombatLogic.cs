using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface ICityCombatLogic {

        #region methods

        int GetCombatStrengthOfCity(ICity city);

        int GetRangedAttackStrengthOfCity(ICity city);

        int GetMaxHealthOfCity(ICity city);

        #endregion

    }

}
