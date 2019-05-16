using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Growth {

    /// <summary>
    /// Determines the food consumption for cities, as well as the food stockpile
    /// necessary for cities to grow and what happens to that stockpile when the
    /// city grows or starves.
    /// </summary>
    public interface IPopulationGrowthLogic  {

        #region methods

        /// <summary>
        /// Determines the amount of food that the city should consume every turn.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>How much food that city should consume.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever city is null</exception>
        int GetFoodConsumptionPerTurn(ICity city);

        /// <summary>
        /// Determines the stockpile of food the city much achieve before it can grow.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>The necessary stockpile.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever city is null</exception>
        int GetFoodStockpileToGrow(ICity city);

        /// <summary>
        /// Determines how much food is consumed from the stockpile when the given city grows.
        /// </summary>
        /// <param name="city">The city in question</param>
        /// <returns>The amount to subtract.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever city is null</exception>
        int GetFoodStockpileAfterGrowth(ICity city);

        /// <summary>
        /// Determines how much food the stockpile should have after the city experiences starvation.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>The new stockpile the city should have.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever city is null</exception>
        int GetFoodStockpileAfterStarvation(ICity city);

        float GetFoodStockpileAdditionFromIncome(ICity city, float foodIncome);

        #endregion

    }

}
