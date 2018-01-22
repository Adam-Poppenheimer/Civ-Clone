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

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cityPossessionCanon"></param>
        /// <param name="name"></param>
        [Inject]
        public Civilization(
            ICivilizationConfig config,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            string name = ""
        ){
            Config              = config;
            CityPossessionCanon = cityPossessionCanon;
            Name                = name;

            TechQueue = new Queue<ITechDefinition>();
        }

        #endregion

        #region instance methods

        /// <inheritdoc/>
        public void PerformIncome() {
            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(this)) {
                GoldStockpile    += Mathf.FloorToInt(city.LastIncome[ResourceType.Gold]);
                CultureStockpile += Mathf.FloorToInt(city.LastIncome[ResourceType.Culture]);
            }
        }

        #endregion

    }

}
