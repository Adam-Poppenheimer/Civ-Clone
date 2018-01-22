using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilization.
    /// </summary>
    public class Civilization : ICivilization {

        #region instance fields and properties

        #region from ICivilization

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public Color Color { get; set; }

        /// <inheritdoc/>
        public int GoldStockpile    { get; set; }

        /// <inheritdoc/>
        public int CultureStockpile { get; set; }

        public Queue<ITechDefinition> TechQueue { get; set; }

        #endregion

        private ICivilizationConfig Config;

        private ICivilizationYieldLogic YieldLogic;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        [Inject]
        public Civilization(
            ICivilizationConfig config, ICivilizationYieldLogic yieldLogic,
            string name = ""
        ){
            Config     = config;
            YieldLogic = yieldLogic;
            Name       = name;

            TechQueue = new Queue<ITechDefinition>();
        }

        #endregion

        #region instance methods

        /// <inheritdoc/>
        public void PerformIncome() {
            var yield = YieldLogic.GetYieldOfCivilization(this);

            GoldStockpile    += Mathf.FloorToInt(yield[ResourceType.Gold]);
            CultureStockpile += Mathf.FloorToInt(yield[ResourceType.Culture]);
        }

        #endregion

    }

}
