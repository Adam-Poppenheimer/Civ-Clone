using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class Civilization : ICivilization {

        #region instance fields and properties

        #region from ICivilization

        public string Name { get; private set; }

        public int GoldStockpile    { get; set; }
        public int CultureStockpile { get; set; }

        #endregion

        private ICivilizationConfig Config;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public Civilization(ICivilizationConfig config, IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            string name = "") {
            Config = config;
            CityPossessionCanon = cityPossessionCanon;
            Name = name;
        }

        #endregion

        #region instance methods

        public void PerformIncome() {
            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(this)) {
                GoldStockpile    += Mathf.FloorToInt(city.LastIncome[ResourceType.Gold]);
                CultureStockpile += Mathf.FloorToInt(city.LastIncome[ResourceType.Culture]);
            }
        }

        #endregion

    }

}
