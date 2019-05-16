using System;

namespace Assets.Simulation.Cities {

    public interface IUnemploymentLogic {

        #region methods

        int GetUnemployedPeopleInCity(ICity city);

        #endregion

    }

}