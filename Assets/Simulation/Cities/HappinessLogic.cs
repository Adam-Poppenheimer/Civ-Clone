using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class HappinessLogic : IHappinessLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private ICityResourceAssignmentCanon ResourceAssignmentCanon;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public HappinessLogic(ICityConfig config, ICityResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config                  = config;
            ResourceAssignmentCanon = resourceAssignmentCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IHappinessLogic

        public int GetHappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int retval = Config.BaseHappiness - city.Population;

            retval += ResourceAssignmentCanon.GetAllResourcesAssignedToCity(city)
                .Where(resource => resource.Type == SpecialtyResourceType.Luxury)
                .Count();

            retval += BuildingPossessionCanon.GetPossessionsOfOwner(city).Sum(building => building.Happiness);

            return retval;
        }

        #endregion

        #endregion

    }

}
