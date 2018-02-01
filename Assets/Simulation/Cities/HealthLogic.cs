using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class HealthLogic : IHealthLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        public HealthLogic(ICityConfig config,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config = config;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityHealthLogic

        public int GetHealthOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int health = Config.BaseHealth - city.Population;

            health += BuildingPossessionCanon.GetPossessionsOfOwner(city).Sum(building => building.Health);

            return health;
        }

        #endregion

        #endregion
        
    }

}
