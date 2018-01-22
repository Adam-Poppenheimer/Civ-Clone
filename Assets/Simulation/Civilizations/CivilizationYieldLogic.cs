using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Civilizations {

    public class CivilizationYieldLogic : ICivilizationYieldLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public CivilizationYieldLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IResourceGenerationLogic resourceGenerationLogic
        ){
            CityPossessionCanon     = cityPossessionCanon;
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #endregion

        #region instance methods

        #region from ICivilizationYieldLogic

        public ResourceSummary GetYieldOfCivilization(ICivilization civilization) {
            ResourceSummary retval = ResourceSummary.Empty;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civilization)) {
                retval += ResourceGenerationLogic.GetTotalYieldForCity(city);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
