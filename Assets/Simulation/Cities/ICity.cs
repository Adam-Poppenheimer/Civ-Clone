using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The base interface for all City objects.
    /// </summary>
    public interface ICity {

        #region properties

        string Name { get; set; }

        Vector3 Position { get; }

        /// <summary>
        /// How many citizens the city has to assign to slots.
        /// </summary>
        int Population { get; set; }

        /// <summary>
        /// The amount of food currently stockpiled in the city.
        /// </summary>
        int FoodStockpile { get; set; }

        /// <summary>
        /// The amount of culture currently stockpiled in the city.
        /// </summary>
        int CultureStockpile { get; set; }

        /// <summary>
        /// The income the city generated during the last call to <ref>PerformIncome</ref>.
        /// </summary>
        YieldSummary LastIncome { get; }

        /// <summary>
        /// The project the city is currently trying to construct.
        /// </summary>
        IProductionProject ActiveProject { get; set; }

        /// <summary>
        /// The current resource focus of the city, which determines what tiles the city will
        /// prioritize when assigning its citizens to slots
        /// </summary>
        YieldFocusType YieldFocus { get; set; }

        /// <summary>
        /// The cell that the city is actively seeking to acquire via border expansion.
        /// </summary>
        IHexCell CellBeingPursued { get; }

        IUnit CombatFacade { get; }

        #endregion

        #region instance methods

        /// <summary>
        /// Performs a single turn's worth of growth and starvation logic, increasing or
        /// reducing the city's population if it's met the conditions for growth or starvation.
        /// </summary>
        /// <remarks>
        /// This method, like the other Perform methods, is intended to be called once per round
        /// during normal execution, though it can be called at arbitrary times without technical
        /// issues.
        /// </remarks>
        void PerformGrowth();

        /// <summary>
        /// Performs a single turn's worth of production, progressing through the ActiveProject
        /// and executing it should it complete.
        /// </summary>
        /// /// <remarks>
        /// This method, like the other Perform methods, is intended to be called once per round
        /// during normal execution, though it can be called at arbitrary times without technical
        /// issues.
        /// </remarks>
        void PerformProduction();

        /// <summary>
        /// Performs a single turn's worth of expansion. This should control the value of
        /// CellBeingPursued and slowly add new territory to the city if it's generating
        /// a sufficient amount of culture.
        /// </summary>
        /// <remarks>
        /// This method, like the other Perform methods, is intended to be called once per round
        /// during normal execution, though it can be called at arbitrary times without technical
        /// issues.
        /// </remarks>
        void PerformExpansion();

        /// <summary>
        /// Redistributes all of the city's citizens amongst its available slots, as determined by
        /// the city's ResourceFocus.
        /// </summary>
        /// <remarks>
        /// Unlike the other Perform methods, this method is often called outside of the normal turn
        /// structure.
        /// </remarks>
        void PerformDistribution();

        /// <summary>
        /// Performs a single turn's worth of income, resetting LastIncome and modifying CultureStockpile
        /// and Foodstockpile.
        /// </summary>
        /// <remarks>
        /// This method, like the other Perform methods, is intended to be called once per round
        /// during normal execution, though it can be called at arbitrary times without technical
        /// issues.
        /// </remarks>
        void PerformIncome();

        void Destroy();

        #endregion

    }

}
