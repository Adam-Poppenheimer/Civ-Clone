using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The base interface for civilizations, which represent the largest agent in the game.
    /// Every player controls a single civilization, which in turn controls a number of cities
    /// and units.
    /// </summary>
    public interface ICivilization {

        #region properties

        /// <summary>
        /// The civilization's non-ID name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The color of the civilization's borders, cities, and units.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// The current amount of gold the civilization has to spend.
        /// </summary>
        int GoldStockpile    { get; set; }

        /// <summary>
        /// The current amount of culture the civilization has to spend.
        /// </summary>
        int CultureStockpile { get; set; }

        Queue<ITechDefinition> TechQueue { get; }

        #endregion

        #region methods

        /// <summary>
        /// Modifies the current gold and culture stockpile of this civilization
        /// based on the per-turn yield of its cities.
        /// </summary>
        void PerformIncome();

        void PerformResearch();

        void Destroy();

        #endregion

    }

}
