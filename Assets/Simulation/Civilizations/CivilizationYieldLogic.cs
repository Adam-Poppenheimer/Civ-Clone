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

        private IYieldGenerationLogic ResourceYieldLogic;

        #endregion

        #region constructors

        [Inject]
        public CivilizationYieldLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IYieldGenerationLogic resourceGenerationLogic
        ){
            CityPossessionCanon     = cityPossessionCanon;
            ResourceYieldLogic = resourceGenerationLogic;
        }

        #endregion

        #region instance methods

        #region from ICivilizationYieldLogic

        public YieldSummary GetYieldOfCivilization(ICivilization civilization) {
            YieldSummary retval = YieldSummary.Empty;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civilization)) {
                retval += ResourceYieldLogic.GetTotalYieldForCity(city);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
