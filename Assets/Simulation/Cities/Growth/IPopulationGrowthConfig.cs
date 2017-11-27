using System;

namespace Assets.Simulation.Cities.Growth {

    public interface IPopulationGrowthConfig {

        #region properties

        int FoodConsumptionPerPerson { get; }

        int BaseStockpile { get; }
        int PreviousPopulationCoefficient { get; }
        float PreviousPopulationExponent { get; }

        #endregion

    }

}