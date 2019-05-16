using System;

namespace Assets.Simulation.Cities.Territory {

    public interface IBorderExpansionModifierLogic {

        #region methods

        float GetBorderExpansionModifierForCity(ICity city);

        #endregion

    }

}