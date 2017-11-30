using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class Civilization : ICivilization {

        #region instance fields and properties

        #region from ICivilization

        public int GoldStockpile    { get; set; }
        public int CultureStockpile { get; set; }

        #endregion

        private ICivilizationConfig Config;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public Civilization(ICivilizationConfig config, IPossessionRelationship<ICivilization, ICity> cityPossessionCanon) {
            Config = config;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        public void PerformIncome() {
            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(this)) {
                GoldStockpile    += city.LastIncome[ResourceType.Gold];
                CultureStockpile += city.LastIncome[ResourceType.Culture];
            }
        }

        #endregion

    }

}
